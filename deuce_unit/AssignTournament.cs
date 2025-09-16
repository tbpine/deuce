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
    int noSingle, int noDouble, int sets, int teamSize, List<Player> players,  int groupSize = 4)
    {

        Organization organization = new Organization() { Id = 1, Name = "Test Org" };


        //Assign
        Tournament tournament = new();
        tournament.Type = tournamentType;
        tournament.Label = label;

        //1 for tennis for now.
        tournament.Sport = sport;
        tournament.Details = new TournamentDetail(noSingle, noDouble, sets);
        tournament.Details.TeamSize = teamSize;

        IGameMaker gm = new GameMakerTennis();

        List<Team> selected = new();

        //Teams

        int noTeams = noPlayers / teamSize;

        for (int i = 0; i < noTeams; i++)
        {
            Team team = new Team() { Id = -1, Label = $"team_{i}", Index = i };

            for (int j = 0; j < teamSize; j++)
            {
                Player player = players[i * teamSize + j];
                player.Tournament = tournament; 
                player.Club = organization;
                team.AddPlayer(player);
                //players.Remove(player);
            }

            selected.Add(team);

        }


        //Action
        //Assert
        FactoryDrawMaker fac = new();
        var mm = fac.Create(tournament, gm);
        var draw = mm.Create(selected);
        tournament.Draw = draw;
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
