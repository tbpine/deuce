using deuce.lib;

namespace deuce_unit;

[TestClass]
public class UnitTestsMatchFactory
{

    [TestMethod]
    [DataRow(8)]
    public void Run_Round_Robin_Game_Engine_N_Players_Returns_Dic(int noPlayers)
    {
        //Assign
        Tournament tournament = new Tournament();
        tournament.Type = new TournamentType(4, "Round Robbin");
        //List of players
        List<Player> players = new();

        for (int i = 0; i < noPlayers; i++)
        {
            players.Add(new Player
            {
                Id = i + 1,
                First = $"player_{i}",
                Last = $"player_{i}"
            });
        }

        //Action
        //Assert

        var results = MatchFactoryBase.Run(players, tournament);

        Assert.IsNotNull(results, "Null result");
        Assert.AreEqual<int>(results.Count, noPlayers - 1, "Incorrect rounds");
        Assert.AreEqual<int>(results.First().Value.Count, noPlayers / 2, "Incorrect games");
    }
}