using System.Globalization;
using deuce;
using deuce.lib;
using MySql.Data.MySqlClient;

class TournamentRepo
{
    /// <summary>
    /// Make a random tournament
    /// </summary>
    /// <returns>A tournamnet object</returns>
    public async Task<Tournament> Random(int tournamentType, string label, int noPlayers, int sport,
    int noSingle, int noDouble, int sets, int teamSize)
    {
        //Assign
        Tournament tournament = new Tournament();
        tournament.Type = tournamentType;
        //1 for tennis for now.
        tournament.Sport = sport;
        tournament.Format = new Format(noSingle, noDouble, sets);
        tournament.TeamSize = teamSize;

        IGameMaker gm = new GameMakerTennis();
        //List<Team> teams = await GetTeams(1);
        List<Player> players = await GetPlayers(1);

        List<Team> selected = new();

        //Teams
        Random rand = new();
        Team bye = new Team(-1, "BYE");
        for(int i = 0; i < teamSize; i++) bye.AddPlayer(new Player(){ Id = -1, First = "BYE", Last = "", Index = i, Ranking = 0d});
        
        int noTeams = noPlayers / teamSize;
        if ((noTeams  % 2)>0 ){  noTeams++ ; selected.Add(bye); }
        

        for (int i = 0; i < noTeams; i++)
        {
            Team team = new Team(){ Id = -1, Label = RandomUtil.GetTeam()};

            for(int j = 0; j < teamSize; j++)
            {
                Player player = players[rand.Next() % players.Count];
                team.AddPlayer(player);
                players.Remove(player);
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

    private async Task<List<Team>> GetTeams(int cludId)
    {
        MySqlConnection dbconn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        dbconn.Open();
        Organization club = new Organization() { Id = cludId, Name = "test_club" };
        Filter filter = new() { ClubId = club.Id };
        var dbRepo = FactoryCreateDbRepo.Create<Team>(dbconn, club);

        var teams = await dbRepo!.GetList(filter);

        return teams;


    }

    private async Task<List<Player>> GetPlayers(int cludId)
    {
        MySqlConnection dbconn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        dbconn.Open();
        Organization club = new Organization() { Id = cludId, Name = "test_club" };
        Filter filter = new() { ClubId = club.Id };

        var dbRepo = FactoryCreateDbRepo.Create<Player>(dbconn, club);

        var players = await dbRepo!.GetList(filter);

        return players;

    }

}
