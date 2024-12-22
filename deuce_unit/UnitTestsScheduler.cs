using deuce;
using deuce.ext;

namespace deuce_unit;

[TestClass]
public class UnitTestsScheduler
{

    [TestMethod]
    [DataRow(8)]
    public void Round_Robbin_Single_Player(int noPlayers)
    {
        //Assign
        Tournament tournament = new Tournament();
        tournament.Type = new TournamentType(1, "Round Robbin");
        //1 for tennis for now.
        tournament.Sport = 1;
        tournament.Format = new Format(1, 1);
        tournament.TeamSize = 1;

        IGameMaker gm = new GameMakerTennis();
        //Teams
        List<Team> teams = new();

        for (int i = 0; i < noPlayers; i++)
        {
            Team t = new Team(i + 1, $"team_{i + 1}");

            t.AddPlayer(new Player
            {
                Id = i + 1,
                First = $"player_{i}",
                Last = $"player_{i}"
            });

            teams.Add(t);

        }

        //Action
        //Assert
        FactorySchedulers fac = new();
        var mm = fac.Create(tournament, gm);
        var results = mm.Run(teams);

        Assert.IsNotNull(results, "Null result");
        Assert.AreEqual<int>(results.NoRounds, noPlayers - 1, "Incorrect rounds");
        Assert.AreEqual<int>(results.GetMatches(0)?.Count ?? 0, noPlayers / 2, "Incorrect games");
    }

    [TestMethod]
    [DataRow(4, 4,2,2)]
    [DataRow(4, 4, 4, 4)]
    public void Round_Robbin_Team_2(int noTeams, int teamSize, int noSingles, int noDoubles)
    {
        //Assign
        Tournament tournament = new Tournament();
        tournament.Type = new TournamentType(1, "Round Robbin");
        //1 for tennis for now.
        tournament.Sport = 1;
        Format fmt = new Format(noSingles, noDoubles);
        tournament.Format = fmt;
        tournament.TeamSize = teamSize;

        IGameMaker gm = new GameMakerTennis();
        //Teams
        List<Team> teams = new();

        for (int i = 0; i < noTeams; i++)
        {
            Team t = new Team(i + 1, $"team_{i + 1}");
            for (int j = 0; j < tournament.TeamSize; j++)
            {
                t.AddPlayer(new Player
                {
                    Id = j + i*tournament.TeamSize,
                    First = $"player_{j + i*tournament.TeamSize}",
                    Last = $""
                });
            }

            teams.Add(t);

        }

        //Action
        //Assert
        FactorySchedulers fac = new();
        var mm = fac.Create(tournament, gm);
        var results = mm.Run(teams);

        Assert.IsNotNull(results, "Null result");
        Assert.AreEqual<int>(results.NoRounds, noTeams - 1, "Incorrect rounds");
        Assert.AreEqual<int>(results.GetMatches(0)?.Count ?? 0, (fmt.NoSingles + fmt.NoDoubles) * (noTeams /2) , "Incorrect games");
        Assert.AreEqual<int>(results.NoMatches(), (fmt.NoSingles + fmt.NoDoubles) * (noTeams /2) * (noTeams-1), "Incorrect games");
    }



}