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

            //Assert the tournament draw.

        }

    }
}
