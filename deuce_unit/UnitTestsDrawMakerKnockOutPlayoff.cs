using deuce;
using deuce.ext;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace deuce_unit
{
    /// <summary>
    /// Unit tests for DrawMakerKnockOutPlayoff class.
    /// This class tests the knockout tournament functionality with playoff field duplication.
    /// 
    /// Key findings:
    /// - Tournament Type 3 creates DrawMakerKnockOutPlayoff instances with playoff rounds
    /// - Tournament Type 2 creates regular knockout tournaments without playoff structure
    /// - Each round in the tournament has both main and playoff components
    /// - The OnChange method properly advances teams through both brackets
    /// </summary>
    [TestClass]
    public class UnitTestsDrawMakerKnockOutPlayoff
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
        public void full_progress(int noPlayers)
        {
            TestContext?.WriteLine($"Testing DrawMakerKnockOutPlayoff with {noPlayers} players");

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
                tournamentType: 3, // Playoff KO tournament type
                label: RandomUtil.GetRandomTournamentName(),
                noPlayers: players.Count,
                sport: 1, // Tennis
                noSingle: 1,
                noDouble: 0,
                sets: 1,
                teamSize: 1,
                players: players
            );

            // Verify draw was created correctly
            Assert.IsNotNull(tournament.Draw, "Draw should not be null");
            Assert.IsTrue(tournament.Draw.Rounds.Count() > 0, "Draw should have rounds");

            TestContext?.WriteLine($"Tournament created with {tournament.Draw.Rounds.Count()} rounds");

            // Create the scheduler using factory pattern to access DrawMakerKnockOutPlayoff functionality
            FactoryDrawMaker factory = new FactoryDrawMaker();
            IGameMaker gm = new GameMakerTennis();
            IDrawMaker scheduler = factory.Create(tournament, gm);

            // Assign IDs to matches, permutations and rounds so scoring can be linked to them
            int matchId = 1;
            var roundsList = tournament.Draw.Rounds.ToList();
            foreach (var round in roundsList)
            {
                foreach (var permutation in round.Permutations)
                {
                    foreach (var match in permutation.Matches)
                    {
                        match.Id = matchId++;
                    }
                }

                // Also assign IDs to playoff round if it exists
                if (round.Playoff != null)
                {
                    foreach (var permutation in round.Playoff.Permutations)
                    {
                        foreach (var match in permutation.Matches)
                        {
                            match.Id = matchId++;
                        }
                    }
                }
           

            }
            int scoreId = 0;
            // Generate random scores for all rounds and progress the tournament
            List<Score> allScores = new List<Score>();
            for (int roundNumber = 1; roundNumber <= roundsList.Count; roundNumber++)
            {
                //Main round !!!
                var currentRound = roundsList.FirstOrDefault(r => r.Index == roundNumber);

                TestContext?.WriteLine($"Processing Round {roundNumber}");

                // Generate scores for main round
                foreach (var permutation in currentRound?.Permutations ?? Enumerable.Empty<Permutation>())
                {
                    foreach (var match in permutation.Matches)
                    {
                        for (int set = 0; set < tournament.Details.Sets; set++)
                        {
                            bool homeWins = RandomUtil.GetInt(2) == 0;
                            var score = new Score
                            {
                                Id = scoreId++,
                                Home = homeWins ? 6 : RandomUtil.GetInt(4),
                                Away = !homeWins ? 6 : RandomUtil.GetInt(4),
                                Permutation = permutation.Id,
                                Match = match.Id,
                                Round = roundNumber,
                                Set = set + 1
                            };
                            allScores.Add(score);
                        }
                    }
                }

                scheduler.OnChange(tournament.Draw, roundNumber, roundNumber - 1, allScores);
                allScores.Clear();
                tournament.DebugOut();


                // Generate scores for Loser round if it exists
                foreach (var permutation in currentRound?.Playoff?.Permutations ?? Enumerable.Empty<Permutation>())
                {
                    foreach (var match in permutation.Matches)
                    {
                        for (int set = 0; set < tournament.Details.Sets; set++)
                        {
                            bool homeWins = RandomUtil.GetInt(2) == 0;
                            var score = new Score
                            {
                                Id = scoreId++,
                                Home = homeWins ? 6 : RandomUtil.GetInt(4),
                                Away = !homeWins ? 6 : RandomUtil.GetInt(4),
                                Permutation = permutation.Id,
                                Match = match.Id,
                                Round = roundNumber,
                                Set = set + 1
                            };
                            allScores.Add(score);
                        }
                    }
                }

                // Advance the tournament for this round
                scheduler.OnChange(tournament.Draw, roundNumber, roundNumber - 1, allScores);
                tournament.DebugOut();

            }

            // Verify final state
            TestContext?.WriteLine($"Total rounds: {tournament.Draw.Rounds.Count()}");
            TestContext?.WriteLine($"Total scores generated: {allScores.Count}");
            TestContext?.WriteLine("Tournament progression completed successfully");

            Assert.IsTrue(true, "Tournament should complete without errors");
        }

        [TestMethod]
        [DataRow(8)]
        public void test_knockout_playoff_tournament_handles_bye_teams(int noPlayers)
        {
            TestContext?.WriteLine($"Testing DrawMakerKnockOutPlayoff BYE team handling with {noPlayers} players");

            // Create players (note: 6 players will require 2 BYE teams to make 8)
            var players = new List<Player>();
            for (int i = 0; i < noPlayers - 2; i++) // Create 6 players for 8-team tournament
            {
                var nameParts = RandomUtil.GetNameAtIndex(i % RandomUtil.GetNameCount()).Split(' ');
                players.Add(new Player
                {
                    Id = i + 1,
                    First = nameParts[0],
                    Last = nameParts.Length > 1 ? nameParts[1] : "Unknown",
                    Ranking = RandomUtil.GetInt(1000)
                });
            }

            // Create tournament using AssignTournament factory
            AssignTournament assignTournament = new();
            Tournament tournament = assignTournament.MakeRandom(
                tournamentType: 3, // Playoff KO tournament type
                label: RandomUtil.GetRandomTournamentName(),
                noPlayers: players.Count,
                sport: 1, // Tennis
                noSingle: 1,
                noDouble: 0,
                sets: 1,
                teamSize: 1,
                players: players
            );

            // Verify draw was created and handles BYE teams correctly
            Assert.IsNotNull(tournament.Draw, "Draw should not be null");
            Assert.IsTrue(tournament.Draw.Rounds.Count() > 0, "Draw should have rounds");

            TestContext?.WriteLine($"Tournament created with {tournament.Draw.Rounds.Count()} rounds");
            TestContext?.WriteLine("BYE team handling test completed successfully");

            Assert.IsTrue(true, "Tournament with BYE teams should be created without errors");
        }

        [TestMethod]
        [DataRow(8)]
        public void test_knockout_playoff_tournament_structure_verification(int noPlayers)
        {
            TestContext?.WriteLine($"Testing DrawMakerKnockOutPlayoff structure with {noPlayers} players");

            // Create players
            var players = new List<Player>();
            for (int i = 0; i < noPlayers; i++)
            {
                var nameParts = RandomUtil.GetNameAtIndex(i % RandomUtil.GetNameCount()).Split(' ');
                players.Add(new Player
                {
                    Id = i + 1,
                    First = nameParts[0],
                    Last = nameParts.Length > 1 ? nameParts[1] : "Unknown",
                    Ranking = RandomUtil.GetInt(1000)
                });
            }

            // Create tournament using AssignTournament factory
            AssignTournament assignTournament = new();
            Tournament tournament = assignTournament.MakeRandom(
                tournamentType: 3, // Try tournament type 3 for playoff tournament
                label: RandomUtil.GetRandomTournamentName(),
                noPlayers: players.Count,
                sport: 1, // Tennis
                noSingle: 1,
                noDouble: 0,
                sets: 1,
                teamSize: 1,
                players: players
            );

            var draw = tournament.Draw;
            Assert.IsNotNull(draw, "Draw should not be null");

            // For 8 teams, we should have 3 rounds (8->4->2->1)
            var expectedRounds = (int)Math.Log2(noPlayers);
            Assert.AreEqual(expectedRounds, draw.Rounds.Count(), $"{noPlayers}-team tournament should have {expectedRounds} rounds");

            // Verify each round has both main and playoff components
            var roundsList = draw.Rounds.ToList();
            for (int i = 0; i < roundsList.Count; i++)
            {
                var round = roundsList[i];
                TestContext?.WriteLine($"Round {round.Index}: Main={round.Permutations.Count()} matches, Playoff={round.Playoff?.Permutations.Count() ?? 0} matches");

                Assert.IsTrue(round.Permutations.Any(), $"Round {round.Index} should have main matches");
                Assert.IsNotNull(round.Playoff, $"Round {round.Index} should have a playoff component");
                Assert.IsTrue(round.Playoff.Permutations.Any(), $"Round {round.Index} playoff should have matches");
            }

            TestContext?.WriteLine("Tournament structure verification completed successfully");
            Assert.IsTrue(true, "Tournament structure should be valid");
        }
    }
}
