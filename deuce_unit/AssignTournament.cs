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
    int noSingle, int noDouble, int sets, int teamSize, List<Player> players, int groupSize = 4)
    {



        //Assign
        Tournament tournament = new();
        //1 for tennis for now.
        tournament.Sport = sport;
        tournament.Type = tournamentType;
        IGameMaker gm = new GameMakerTennis();
        FactoryDrawMaker fac = new();
        var dm = fac.Create(tournament, gm);

        tournament.Teams = new();

        return MakeRandom(tournamentType, label, noPlayers, sport,
        noSingle, noDouble, sets, teamSize, players,  gm, groupSize);
    }

    /// <summary>
    /// Make a random tournament
    /// </summary>
    /// <returns>A tournamnet object</returns>
    public Tournament MakeRandom(int tournamentType, string label, int noPlayers, int sport,
    int noSingle, int noDouble, int sets, int teamSize, List<Player> players,  IGameMaker gm, int groupSize = 4)
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
        tournament.Details.GroupSize = groupSize;

        tournament.Teams = new();

        //Teams

        int noTeams = noPlayers / teamSize;
        //Team primary key for draw makers looking for teams
        int teamIdx = 1;

        for (int i = 0; i < noTeams; i++)
        {
            Team team = new Team() { Id = teamIdx++, Label = $"team_{i}", Index = i };

            for (int j = 0; j < teamSize; j++)
            {
                Player player = players[i * teamSize + j];
                player.Tournament = tournament;
                player.Club = organization;
                team.AddPlayer(player);
                //players.Remove(player);
            }

            tournament.Teams.Add(team);

        }
        var fac = new FactoryDrawMaker();
        var dm2 = fac.Create(tournament, gm);
    
        var draw = dm2.Create();
        tournament.Draw = draw;
        return tournament;

    }



}
