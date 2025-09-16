using deuce;
using deuce.ext;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace deuce_unit
{

    [TestClass]
    public class UnitTestsDrawMakerGroups
    {
        public TestContext? TestContext { get; set; }

        [TestInitialize]
        public void Setup()
        {
            // Initialize before each test
        }

        [TestMethod]
        [DataRow(8)]
        [DataRow(16)]
        public void create_tournament_with_teams(int noPlayers)
        {
            TestContext?.WriteLine($"Testing DrawMakerGroup with {noPlayers} players");

            // Create unique random players by shuffling indices
            var players = new List<Player>();
            var availableIndices = Enumerable.Range(0, RandomUtil.GetNameCount()).ToList();

            // Shuffle the indices to ensure random selection without duplicates
            Random random = new Random();
            for (int i = availableIndices.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (availableIndices[i], availableIndices[j]) = (availableIndices[j], availableIndices[i]);
            }

            // Take the first noPlayers indices and create players
            for (int i = 0; i < noPlayers; i++)
            {
                var nameParts = RandomUtil.GetNameAtIndex(availableIndices[i]).Split(' ');
                players.Add(new Player
                {
                    Id = i + 1,
                    First = nameParts[0],
                    Last = nameParts.Length > 1 ? nameParts[1] : "Unknown",
                    Ranking = random.NextDouble() * 100 // Add random ranking for testing
                });
            }

            // Create tournament using AssignTournament factory - this will properly create the DrawMakerKnockOutPlayoff
            AssignTournament assignTournament = new();
            Tournament tournament = assignTournament.MakeRandom(
                tournamentType: 4, // Groups playoff
                label: RandomUtil.GetRandomTournamentName(),
                noPlayers: players.Count,
                sport: 1, // Tennis
                noSingle: 1,
                noDouble: 0,
                sets: 1,
                teamSize: 1,
                players: players
            );


            
            //Assert each group draw
            foreach (var group in tournament.Groups)
            {
                Assert.IsNotNull(group.Draw, "Draw was null");
                //Check that the correct number of rounds were created
                //For 8 players in groups of 4 teams, we expect 3 rounds 2 , 1 and the playoff final

                Assert.IsTrue(group.Draw.NoRounds == (Math.Log2(group.Size) + 1), "Not enough rounds in group draw");
                //Check playoff rounds
                foreach (var round in group.Draw.Rounds)
                {
                    //Ignore the last round
                    if (round.Index == group.Draw.NoRounds - 1) break;
                    //Calculuate the number of matches expected in this round
                    //For round 0 (first round) with 8 players we expect 4 matches
                    int expectedMatches = group.Teams.Count() / (int)Math.Pow(2, round.Index);
                    Assert.AreEqual<int>(expectedMatches, round.Permutations.Count, "Not enough matches in round");
                    //Check perms
                    Assert.IsNotNull(round.Playoff, "Playoff was null");
                    //Check the number of playoff matches
                    int expectedPlayoffMatches = round.Index == 1 ? expectedMatches / 2 : expectedMatches;
                    Assert.AreEqual<int>(expectedPlayoffMatches, round.Playoff.Permutations.Count, "Not enough playoff matches");
                }

            }


        }

    }
}
