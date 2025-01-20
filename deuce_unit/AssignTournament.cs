using deuce;
using MySql.Data.MySqlClient;

class AssignTournament
{
    /// <summary>
    /// Make a random tournament
    /// </summary>
    /// <returns>A tournamnet object</returns>
    public async Task<Tournament> MakeRandom(int tournamentType, string label, int noPlayers, int sport,
    int noSingle, int noDouble, int sets, int teamSize)
    {
        //Remove all players for the tournament
        string sqlDelPlayers = $"DELETE FROM `player` WHERE `organization` = 1";
        DirectSQL.Run(sqlDelPlayers);

        MySqlConnection dbconn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        dbconn.Open();

        Organization organization = new Organization() { Id = 1, Name = "Test Org" };

        //Add enough player to play in the tournament
        for (int i = 0; i < noPlayers; i++)
        {
            DbRepoPlayer dbRepoPlayer = new(dbconn, organization);
            string randomName = RandomUtil.GetNameAtIndex(i);
            //Split names
            string[] names = randomName.Split(" ");
            Player player= new() { Id = 0, Club = organization, First = names[0], Last = names[1], Index = 0,
            Ranking  = 0.0};
            dbRepoPlayer.Set(player);
        }

        //close connnection
        dbconn.Close();

        //Assign
        Tournament tournament = new Tournament();
        tournament.Type = tournamentType;
        //1 for tennis for now.
        tournament.Sport = sport;
        tournament.Format = new Format(noSingle, noDouble, sets);
        tournament.TeamSize = teamSize;

        IGameMaker gm = new GameMakerTennis();

        //--------------------------------------------------
        //Must have enough players in the organization     |
        //--------------------------------------------------
        List<Player> players = await GetPlayers(1);

        List<Team> selected = new();

        //Teams
        
        Team bye = new Team(-1, "BYE");
        for (int i = 0; i < teamSize; i++) bye.AddPlayer(new Player() { Id = -1, First = "BYE", Last = "", Index = i, Ranking = 0d });

        int noTeams = noPlayers / teamSize;
        if ((noTeams % 2) > 0) { noTeams++; selected.Add(bye); }


        for (int i = 0; i < noTeams; i++)
        {
            Team team = new Team() { Id = -1, Label = $"team_{i}" ,Index = i};

            for (int j = 0; j < teamSize; j++)
            {
                Player player = players[i*teamSize + j]; 
                team.AddPlayer(player);
                //players.Remove(player);
            }

            selected.Add(team);

        }


        //Action
        //Assert
        FactorySchedulers fac = new();
        var mm = fac.Create(tournament, gm);
        var schedule = mm.Run(selected);
        tournament.Schedule = schedule;
        tournament.Teams = selected;
        return tournament;

    }


    private async Task<List<Player>> GetPlayers(int cludId)
    {
        MySqlConnection dbconn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        dbconn.Open();
        Organization club = new Organization() { Id = cludId, Name = "test_club" };
        Filter filter = new() { ClubId = club.Id };

        
        DbRepoPlayer dbRepoPlayer = new DbRepoPlayer(dbconn,club);

        var players = await dbRepoPlayer.GetList(filter);

        dbconn.Close();

        return players;

    }

}
