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
            int grpSize = 4;
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
                players: players,
                groupSize: grpSize // Groups of 4
            );


            //Check that the correct number of groups were created
            Assert.AreEqual(tournament.Groups.Count(), noPlayers / grpSize, "Not enough groups created");

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

            foreach (var group in tournament.Groups)
            {
                TestContext?.WriteLine($"Group {group.Label}:");
                TestContext?.WriteLine($"  Teams: {group.Teams.Count()}");
                TestContext?.WriteLine($"  Draw Rounds: {group.Draw?.NoRounds}");
                for (int r = 0; r < group.Draw?.NoRounds; r++)
                {
                    var round = group.Draw.GetRoundAtIndex(r);
                    TestContext?.WriteLine($"Round {r}: Matches={round.Permutations.Count}, Playoff Matches={round.Playoff?.Permutations.Count}");
                }
            }
            //Check the main round which consists winners of each group
            //Check the number of rounds in the main draw
            Assert.IsNotNull(tournament.Draw, "Main Draw was null");
            //There's two winners from each group
            //So the number of entries in the main draw is 2 * number of groups

            Assert.AreEqual<int>((int)Math.Log2(tournament.Groups.Count() * 2.0), tournament.Draw.NoRounds, "Not enough rounds in main draw");
            //Check main draw rounds
            for (int r = 0; r < tournament.Draw.NoRounds; r++)
            {
                var round = tournament.Draw.GetRoundAtIndex(r);
                int expectedMatches = (tournament.Groups.Count()) / (int)Math.Pow(2, r);
                Assert.AreEqual<int>(expectedMatches, round.Permutations.Count, "Not enough matches in main draw round");
            }

        }

        [TestMethod]
        [DataRow(8)]
        [DataRow(16)]
        public void progression_with_groups(int noPlayers)
        {
            TestContext?.WriteLine($"Testing basic tournament progression with groups for {noPlayers} players");

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

            int grpSize = 4;
            // Create tournament using AssignTournament factory - this will properly create the DrawMakerGroups
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
                players: players,
                groupSize: grpSize // Groups of 4
            );

            // Verify tournament structure
            Assert.IsNotNull(tournament.Groups, "Groups should not be null");
            Assert.AreEqual(noPlayers / grpSize, tournament.Groups.Count(), "Incorrect number of groups");
            Assert.IsNotNull(tournament.Draw, "Main tournament draw should not be null");

            // Create the scheduler using factory pattern
            FactoryDrawMaker factory = new FactoryDrawMaker();
            IGameMaker gm = new GameMakerTennis();
            IDrawMaker scheduler = factory.Create(tournament, gm);

            // Assign IDs to all matches in groups and main draw
            int matchId = 1;

            // Assign IDs to group matches
            foreach (var group in tournament.Groups)
            {
                Assert.IsNotNull(group.Draw, $"Group {group.Label} should have a draw");
                foreach (var round in group.Draw.Rounds)
                {

                    int rPermId = 0;
                    foreach (var permutation in round.Permutations)
                    {
                        permutation.Id = rPermId++;
                        foreach (var match in permutation.Matches)
                        {
                            match.Id = matchId++;
                        }
                    }
                    // Also assign IDs to playoff rounds within groups
                    if (round.Playoff != null)
                    {
                        rPermId = 0;
                        foreach (var permutation in round.Playoff.Permutations)
                        {
                            permutation.Id = rPermId++;
                            foreach (var match in permutation.Matches)
                            {
                                match.Id = matchId++;
                            }
                        }
                    }
                }
            }

            // Assign IDs to main draw matches
            foreach (var round in tournament.Draw.Rounds)
            {
                int permId = 0;
                foreach (var permutation in round.Permutations)
                {
                    permutation.Id = permId++;
                    foreach (var match in permutation.Matches)
                    {
                        match.Id = matchId++;
                    }
                }
            }

            // Test basic progression capability by validating tournament structure
            TestContext?.WriteLine("Testing tournament structure and basic functionality...");

            // Verify that groups have proper draws
            foreach (var group in tournament.Groups)
            {
                Assert.IsNotNull(group.Draw, $"Group {group.Label} should have a draw");
                Assert.IsTrue(group.Draw.Rounds.Count() > 0, $"Group {group.Label} should have rounds");

                // Verify each round in the group has matches
                foreach (var round in group.Draw.Rounds)
                {
                    Assert.IsTrue(round.Permutations.Count() > 0, $"Group {group.Label} round should have permutations");
                    foreach (var permutation in round.Permutations)
                    {
                        Assert.IsTrue(permutation.Matches.Count() > 0, $"Group {group.Label} permutation should have matches");
                    }
                }
            }

            // Verify main draw structure
            Assert.IsTrue(tournament.Draw.Rounds.Count() > 0, "Main draw should have rounds");
            foreach (var round in tournament.Draw.Rounds)
            {
                Assert.IsTrue(round.Permutations.Count() > 0, "Main draw round should have permutations");
                foreach (var permutation in round.Permutations)
                {
                    Assert.IsTrue(permutation.Matches.Count() > 0, "Main draw permutation should have matches");
                }
            }

            // Test that we can create scores for matches (this validates the match structure)
            TestContext?.WriteLine("Validating match scoring structure...");
            var firstGroup = tournament.Groups.First();
            Assert.IsNotNull(firstGroup.Draw, "First group should have a draw");
            var firstRound = firstGroup.Draw.GetRoundAtIndex(0);

            if (firstRound.Permutations.Any() && firstRound.Permutations.First().Matches.Any())
            {
                var firstMatch = firstRound.Permutations.First().Matches.First();

                // Verify match has proper structure for scoring
                Assert.IsNotNull(firstMatch.Home, "Match should have home team");
                Assert.IsNotNull(firstMatch.Away, "Match should have away team");

                TestContext?.WriteLine("Match structure validation passed");
            }

            // Add random scores and progress the tournament
            TestContext?.WriteLine("Adding random scores and progressing tournament...");
            List<Score> allScores = new List<Score>();
            int scoreId = 1;

            ///----------------------------------------------------------
            ///Scores
            /// 1. For main draw in each group
            /// 2. For playoff in each group
            /// 3. For main tournament draw
            ///----------------------------------------------------------
            /// 
            

            foreach (var group in tournament.Groups)
            {
                TestContext?.WriteLine($"Progressing Group {group.Label}");
                Assert.IsNotNull(group.Draw, $"Group {group.Label} should have a draw");
                var groupRounds = group.Draw.Rounds.ToList();

                for (int roundIndex = 0; roundIndex < groupRounds.Count-1; roundIndex++)
                {
                    var currentRound = groupRounds[roundIndex];
                    List<Score> roundScores = new List<Score>();

                    TestContext?.WriteLine($"  Processing Group {group.Label} Round {currentRound.Index}");

                    // Generate scores for main round matches
                    foreach (var permutation in currentRound.Permutations)
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
                                    Round = currentRound.Index,
                                    Set = set + 1
                                };
                                roundScores.Add(score);
                            }
                        }
                    }
                    //Progress before playoff
                    // Generate scores for playoff matches if they exist
                    scheduler.OnChange(group.Draw, currentRound.Index, currentRound.Index - 1, roundScores);
                    //clear scores
                    roundScores.Clear();
                    if (currentRound.Playoff != null)
                    {
                        foreach (var permutation in currentRound.Playoff.Permutations)
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
                                        Round = currentRound.Index,
                                        Set = set + 1
                                    };
                                    roundScores.Add(score);
                                }
                            }
                        }
                    }

                    allScores.AddRange(roundScores);

                    // Progress the group after this round
                    if (roundScores.Any())
                    {
                        scheduler.OnChange(group.Draw, currentRound.Index, currentRound.Index - 1, roundScores);
                    }
                }
            }

            // Progress the main tournament draw
            TestContext?.WriteLine("Progressing Main Tournament Draw");
            //Group stage to main stage
            scheduler.OnChange(tournament.Draw, 1, 1, new List<Score>());

            var mainRounds = tournament.Draw.Rounds.ToList();

            for (int roundIndex = 0; roundIndex < mainRounds.Count; roundIndex++)
            {
                var currentRound = mainRounds[roundIndex];
                List<Score> roundScores = new List<Score>();

                TestContext?.WriteLine($"  Processing Main Draw Round {currentRound.Index}");

                // Generate scores for main round matches
                foreach (var permutation in currentRound.Permutations)
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
                                Round = currentRound.Index,
                                Set = set + 1
                            };
                            roundScores.Add(score);
                        }
                    }
                }

                allScores.AddRange(roundScores);

                // Progress the main draw after this round
                if (roundScores.Any())
                {
                    scheduler.OnChange(tournament.Draw, currentRound.Index+1, currentRound.Index, roundScores);
                }
            }

            TestContext?.WriteLine($"Generated {allScores.Count} total scores");
            TestContext?.WriteLine("Tournament progression with random scores completed successfully");
        }

    }
}
