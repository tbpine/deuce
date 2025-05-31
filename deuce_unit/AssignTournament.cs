using System.Net;
using deuce;
using MySql.Data.MySqlClient;

class AssignTournament
{
    /// <summary>
    /// Make a random tournament
    /// </summary>
    /// <returns>A tournamnet object</returns>
    public Tournament MakeRandom(int tournamentType, string label, int noPlayers, int sport,
    int noSingle, int noDouble, int sets, int teamSize)
    {

        Organization organization = new Organization() { Id = 1, Name = "Test Org" };


        //Assign
        Tournament tournament = new Tournament();
        tournament.Type = tournamentType;
        tournament.Label = label;

        //1 for tennis for now.
        tournament.Sport = sport;
        tournament.Details = new TournamentDetail(noSingle, noDouble, sets);
        tournament.Details.TeamSize = teamSize;

        IGameMaker gm = new GameMakerTennis();

        //--------------------------------------------------
        //Must have enough players in the organization     |
        //--------------------------------------------------
        List<Player> players = new();// await GetPlayers(1);
                                     //Add enough player to play in the tournament
        for (int i = 0; i < noPlayers; i++)
        {

            string randomName = RandomUtil.GetNameAtIndex(i);
            //Split names
            string[] names = randomName.Split(" ");
            Player player = new()
            {
                Id = 0,
                Club = organization,
                First = names[0],
                Last = names[1],
                Middle = "",
                Index = i,
                Ranking = 0.0,
            };
            players.Add(player);
        }

        List<Team> selected = new();

        //Teams

        Team bye = new Team(-1, "BYE");
        for (int i = 0; i < teamSize; i++) bye.AddPlayer(new Player() { Id = -1, First = "BYE", Last = "", Index = i, Ranking = 0d });

        int noTeams = noPlayers / teamSize;
        if ((noTeams % 2) > 0) { noTeams++; selected.Add(bye); }


        for (int i = 0; i < noTeams; i++)
        {
            Team team = new Team() { Id = -1, Label = $"team_{i}", Index = i };

            for (int j = 0; j < teamSize; j++)
            {
                Player player = players[i * teamSize + j];
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


    private async Task<List<Player>> GetPlayers(int cludId, int tournamentId)
    {
        MySqlConnection dbconn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        dbconn.Open();
        Filter filter = new() { TournamentId = tournamentId};


        DbRepoPlayer dbRepoPlayer = new DbRepoPlayer(dbconn);
        

        var players = await dbRepoPlayer.GetList(filter);

        dbconn.Close();

        return players;

    }

}
