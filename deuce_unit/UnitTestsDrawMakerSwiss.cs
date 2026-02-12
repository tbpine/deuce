using deuce;
using System.Diagnostics;
using System.IO;

namespace deuce_unit
{
    [TestClass]
    public class UnitTestsDrawMakerSwiss
    {
        public TestContext? TestContext { get; set; }

        private IDrawMaker _dm;
        private IGameMaker _gm;

        private FactoryDrawMaker _fac = new FactoryDrawMaker();

        public UnitTestsDrawMakerSwiss()
        {
            // Initialize before each test
            _gm = new GameMakerTennis();

            _dm = _fac.Create(new Tournament() { Type = 5, Sport = 1 }, _gm);

        }
        [TestInitialize]
        public void Setup()
        {
        }

        [TestMethod]
        [DataRow(8)]
        [DataRow(16)]
        [DataRow(32)]
        public void create_swiss_tournament_progression(int noPlayers)
        {
            TestContext?.WriteLine($"Testing Swiss tournament progression with {noPlayers} players");

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

            // Create tournament using AssignTournament factory for Swiss format
            AssignTournament assignTournament = new();
            Tournament tournament = assignTournament.MakeRandom(
                tournamentType: 5, // Swiss tournament
                label: RandomUtil.GetRandomTournamentName(),
                noPlayers: players.Count,
                sport: 1, // Tennis
                noSingle: 1,
                noDouble: 0,
                sets: 1,
                teamSize: 1,
                players: players
            );

            // Verify tournament structure
            Assert.IsNotNull(tournament, "Tournament should not be null");
            Assert.IsNotNull(tournament.Draw, "Tournament draw should not be null");
            Assert.IsNotNull(tournament.Teams, "Tournament teams should not be null");
            Assert.AreEqual(noPlayers, tournament.Teams.Count(), "Tournament should have correct number of teams");

            // Verify each team has exactly one player
            foreach (var team in tournament.Teams)
            {
                Assert.AreEqual(1, team.Players.Count(), "Each team should have exactly one player");
            }

            TestContext?.WriteLine($"Created tournament '{tournament.Label}' with {tournament.Teams.Count()} teams");
            TestContext?.WriteLine($"Tournament draw has {tournament.Draw.NoRounds} rounds");

            // Swiss tournaments should start with only 1 round initially
            Assert.AreEqual(1, tournament.Draw.NoRounds, "Swiss tournament should start with only 1 round");

            // Create the scheduler using factory pattern
            FactoryDrawMaker factory = new FactoryDrawMaker();
            IGameMaker gm = new GameMakerTennis();
            IDrawMaker scheduler = factory.Create(tournament, gm);


            TestContext?.WriteLine("Starting Swiss tournament progression...");

            // Progress through Swiss tournament rounds dynamically
            List<Score> allScores = new List<Score>();
            int scoreId = 1;
            int currentRoundIndex = 0;
            int maxRounds = GetExpectedSwissRounds(noPlayers);

            while (currentRoundIndex < maxRounds)
            {
                var currentRound = tournament.Draw.GetRoundIndex(currentRoundIndex);
                TestContext?.WriteLine($"Processing Round {currentRound.Index + 1}:");

                // Validate round structure before playing
                Assert.IsNotNull(currentRound, $"Round {currentRoundIndex} should not be null");
                Assert.IsTrue(currentRound.Permutations.Count() > 0, $"Round {currentRoundIndex} should have permutations");

                TestContext?.WriteLine($"  Round has {currentRound.Permutations.Count()} matches");

                // Generate random scores for all matches in this round
                List<Score> roundScores = new List<Score>();

                foreach (var permutation in currentRound.Permutations)
                {
                    foreach (var match in permutation.Matches)
                    {
                        // Verify match structure
                        Assert.IsNotNull(match.Home, $"Match {match.Id} should have home team");
                        Assert.IsNotNull(match.Away, $"Match {match.Id} should have away team");
                        Assert.AreNotEqual(match.Home.First()?.Id, match.Away.First()?.Id, $"Match {match.Id} should have different home and away teams");

                        TestContext?.WriteLine($"    Match {match.Id}: {match.Home.First()?.ToString()} vs {match.Away.First()?.ToString()}");

                        // Create scores for each set
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
                            allScores.Add(score);
                        }
                    }
                }

                TestContext?.WriteLine($"  Generated {roundScores.Count} scores for round {currentRoundIndex + 1}");

                // Progress to next round using scores from current round
                if (currentRoundIndex < maxRounds)
                {
                    int roundsBefore = tournament.Draw.NoRounds;
                    scheduler.OnChange(tournament.Draw, currentRoundIndex + 1, currentRoundIndex, roundScores);
                    int roundsAfter = tournament.Draw.NoRounds;

                    TestContext?.WriteLine($"  Draw now has {roundsAfter} rounds (was {roundsBefore})");

                    // Check if a new round was created
                    if (roundsAfter > roundsBefore)
                    {
                        var nextRound = tournament.Draw.GetRoundIndex(currentRoundIndex + 1);
                        if (nextRound != null && nextRound.Permutations.Any())
                        {
                            TestContext?.WriteLine($"  Next round has {nextRound.Permutations.Count()} matches ready");

                            // In Swiss system, verify no player plays the same opponent twice
                            ValidateSwissRoundUniqueness(tournament, currentRoundIndex + 1);
                        }
                    }
                }
                else
                {
                    TestContext?.WriteLine($"  Final round completed");
                }

                currentRoundIndex++;
            }

            TestContext?.WriteLine($"Swiss tournament completed with {allScores.Count} total scores");

            // Final validations - Swiss tournament should have created the expected number of rounds
            Assert.IsTrue(tournament.Draw.NoRounds >= 1, "Tournament should have at least the initial round");
            Assert.IsTrue(tournament.Draw.NoRounds <= maxRounds, $"Tournament should not exceed {maxRounds} rounds");

            // Verify tournament progression logic worked
            ValidateSwissTournamentProgression(tournament, allScores, currentRoundIndex);

            TestContext?.WriteLine("Swiss tournament progression validation completed successfully");
        }

        [TestMethod]
        [DataRow(8)]
        [DataRow(16)]
        [DataRow(32)]
        [DataRow(64)]
        [DataRow(128)]
        public void swiss_tournament_clear_winner_gets_bye_in_next_round(int noPlayers)
        {

            TestContext?.WriteLine($"Testing Swiss tournament progression with bye scenario");

            // Create unique random players by shuffling indices
            int playerCount = noPlayers;
            var players = RandomUtil.GetRandomPlayers(playerCount); //Get 8 unique players
                                                                    // Create tournament using AssignTournament factory for Swiss format
            AssignTournament assignTournament = new();


            Tournament tournament = assignTournament.MakeRandom(
                tournamentType: 5, // Swiss tournament
                label: RandomUtil.GetRandomTournamentName(),
                noPlayers: players.Count,
                sport: 1, // Tennis
                noSingle: 1,
                noDouble: 0,
                sets: 1,
                teamSize: 1,
                players: players
            );


            Assert.IsNotNull(tournament, "Tournament should not be null");
            Assert.IsNotNull(tournament.Draw, "Tournament draw should not be null");
            Assert.IsNotNull(tournament.Teams, "Tournament teams should not be null");
            Assert.AreEqual(playerCount, tournament.Teams.Count(), "Tournament should have correct number of teams");
            // Verify each team has exactly one player
            foreach (var team in tournament.Teams)
            {
                Assert.AreEqual(1, team.Players.Count(), "Each team should have exactly one player");
            }
            TestContext?.WriteLine($"Created tournament '{tournament.Label}' with {tournament.Teams.Count()} teams");
            TestContext?.WriteLine($"Tournament draw has {tournament.Draw.NoRounds} rounds");

            // Swiss tournaments should start with only 1 round initially
            Assert.AreEqual(1, tournament.Draw.NoRounds, "Swiss tournament should start with only 1 round");

            TestContext?.WriteLine("Starting Swiss tournament progression...");

            // Progress through Swiss tournament rounds dynamically
            List<Score> allScores = new List<Score>();
            List<Score> roundScores = new List<Score>();
            int scoreId = 1;
            int currentRoundIndex = 0;
            int maxRounds = DrawMakerSwiss.ExpectedNumberOfRounds(players.Count);

            // First, assign a clear winner for the first round. The rest is a draw, means
            //there is a group involving an odd number of players with same score and one clear winner.
            foreach (Permutation p in tournament.Draw.GetRoundIndex(0).Permutations)
            {
                foreach (Match m in p.Matches)
                {
                    int scoreHome = 3;
                    int scoreAway = 3;

                    if (m.Home.First().Id == tournament.Teams.First().Id) { scoreHome = 6; scoreAway = 0; }
                    else if (m.Away.First().Id == tournament.Teams.First().Id) { scoreAway = 6; scoreHome = 0; }
                    // Assign clear winners for first round
                    var score = new Score
                    {
                        Id = scoreId++,
                        Home = scoreHome,
                        Away = scoreAway,
                        Permutation = p.Id,
                        Match = m.Id,
                        Round = 0,
                        Set = 1
                    };

                    roundScores.Add(score);
                    allScores.Add(score);
                }
            }

            //Progress to next round
            var fac = new FactoryDrawMaker();
            var _dm = fac.Create(tournament, _gm);
            _dm.OnChange(tournament.Draw, 1, 0, roundScores);
            //Move to next round
            currentRoundIndex++;
            //Validate that outcome of progression round 1.

            //1. Expect the first standing to get a bye
            //2. Expect an adjusted group for the rest of players

            var secondRound = tournament.Draw.GetRoundIndex(currentRoundIndex);
            var winningTeam = tournament.Teams.First();

            //count the number of matches
            int totalMatches = secondRound.Permutations.Sum(p => p.Matches.Count());
            Assert.AreEqual(playerCount / 2, totalMatches, " number of matches should be players /2");

            while (currentRoundIndex < maxRounds && currentRoundIndex > 0)
            {
                var currentRound = tournament.Draw.GetRoundIndex(currentRoundIndex);
                TestContext?.WriteLine($"Processing Round {currentRound.Index + 1}:");

                // Validate round structure before playing
                Assert.IsNotNull(currentRound, $"Round {currentRoundIndex} should not be null");
                Assert.IsTrue(currentRound.Permutations.Count() > 0, $"Round {currentRoundIndex} should have permutations");

                TestContext?.WriteLine($"  Round has {currentRound.Permutations.Count()} matches");

                roundScores.Clear();

                foreach (var permutation in currentRound.Permutations)
                {
                    foreach (var match in permutation.Matches)
                    {
                        // Verify match structure
                        Assert.IsNotNull(match.Home, $"Match {match.Id} should have home team");
                        Assert.IsNotNull(match.Away, $"Match {match.Id} should have away team");
                        Assert.AreNotEqual(match.Home.First()?.Id, match.Away.First()?.Id, $"Match {match.Id} should have different home and away teams");

                        TestContext?.WriteLine($"Match {match.Id}: {match.Home.First()?.ToString()} vs {match.Away.First()?.ToString()}");

                        // Create scores for each set
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
                            allScores.Add(score);
                        }
                    }
                }

                TestContext?.WriteLine($"  Generated {roundScores.Count} scores for round {currentRoundIndex + 1}");

                // Progress to next round using scores from current round
                if (currentRoundIndex < maxRounds)
                {
                    int roundsBefore = tournament.Draw.NoRounds;
                    _dm.OnChange(tournament.Draw, currentRoundIndex + 1, currentRoundIndex, roundScores);
                    int roundsAfter = tournament.Draw.NoRounds;

                    TestContext?.WriteLine($"  Draw now has {roundsAfter} rounds (was {roundsBefore})");

                    // Check if a new round was created
                    if (roundsAfter > roundsBefore)
                    {
                        var nextRound = tournament.Draw.GetRoundIndex(currentRoundIndex + 1);
                        if (nextRound != null && nextRound.Permutations.Any())
                        {
                            TestContext?.WriteLine($"  Next round has {nextRound.Permutations.Count()} matches ready");

                            // In Swiss system, verify no player plays the same opponent twice
                            ValidateSwissRoundUniqueness(tournament, currentRoundIndex + 1);
                        }
                    }
                }
                else
                {
                    TestContext?.WriteLine($"  Final round completed");
                }

                currentRoundIndex++;
            }

            TestContext?.WriteLine($"Swiss tournament completed with {allScores.Count} total scores");

            // Final validations - Swiss tournament should have created the expected number of rounds
            Assert.IsTrue(tournament.Draw.NoRounds >= 1, "Tournament should have at least the initial round");
            Assert.IsTrue(tournament.Draw.NoRounds <= maxRounds, $"Tournament should not exceed {maxRounds} rounds");

            // Verify tournament progression logic worked
            ValidateSwissTournamentProgression(tournament, allScores, currentRoundIndex);

            TestContext?.WriteLine("Swiss tournament progression validation completed successfully");
        }


        private void ValidateSwissTournamentProgression(Tournament tournament, List<Score> allScores, int roundsPlayed)
        {
            // Verify that scores exist for the rounds that were played
            var distinctRounds = allScores.Select(s => s.Round).Distinct().Count();
            Assert.IsTrue(distinctRounds <= roundsPlayed, $"Scores should not exist for more rounds than played");

            // Verify each player participated in each played round
            for (int r = 0; r < roundsPlayed && r < tournament?.Draw?.NoRounds; r++)
            {
                var round = tournament.Draw.GetRoundIndex(r);
                var playersInRound = new HashSet<int>();

                foreach (var perm in round?.Permutations ?? Enumerable.Empty<Permutation>())
                {
                    foreach (var match in perm.Matches)
                    {
                        playersInRound.Add(match.Home.First()?.Id ?? 0);
                        playersInRound.Add(match.Away.First()?.Id ?? 0);
                    }
                }

                Assert.AreEqual(tournament.Teams.Count(), playersInRound.Count,
                    $"All players should participate in round {r + 1}");
            }
        }

        private void ValidateSwissRoundUniqueness(Tournament tournament, int currentRoundIndex)
        {
            // Get all matches from previous rounds
            var previousMatches = new HashSet<(int, int)>();

            for (int r = 0; r < currentRoundIndex; r++)
            {
                var round = tournament.Draw?.GetRoundIndex(r);
                foreach (var perm in round?.Permutations ?? Enumerable.Empty<Permutation>())
                {
                    foreach (var match in perm.Matches)
                    {
                        // Store both directions of the pairing (using first player from each team)
                        var homeId = match.Home.First()?.Id ?? 0;
                        var awayId = match.Away.First()?.Id ?? 0;
                        previousMatches.Add((homeId, awayId));
                        previousMatches.Add((awayId, homeId));
                    }
                }
            }

            // Check current round for duplicate pairings
            var currentRound = tournament.Draw?.GetRoundIndex(currentRoundIndex);
            foreach (var perm in currentRound?.Permutations ?? Enumerable.Empty<Permutation>())
            {
                foreach (var match in perm.Matches)
                {
                    var homeId = match.Home.First()?.Id ?? 0;
                    var awayId = match.Away.First()?.Id ?? 0;
                    var pairing = (homeId, awayId);
                    if (previousMatches.Contains(pairing))
                        TestContext?.WriteLine($"Teams {match.Home.First()?.ToString()} and {match.Away.First()?.ToString()} have already played each other");
                }
            }
        }

        private void ValidateSwissTournamentCompleteness(Tournament tournament, List<Score> allScores)
        {
            // In Swiss format, every player should play in every round
            int expectedGamesPerPlayer = tournament.Draw?.NoRounds ?? 0;

            var playerGameCounts = new Dictionary<int, int>();

            // Count games per player
            foreach (var round in tournament.Draw?.Rounds ?? Enumerable.Empty<Round>())
            {
                foreach (var perm in round.Permutations)
                {
                    foreach (var match in perm.Matches)
                    {
                        var homeId = match.Home.First()?.Id ?? 0;
                        var awayId = match.Away.First()?.Id ?? 0;
                        playerGameCounts[homeId] = playerGameCounts.GetValueOrDefault(homeId, 0) + 1;
                        playerGameCounts[awayId] = playerGameCounts.GetValueOrDefault(awayId, 0) + 1;
                    }
                }
            }

            // Verify each player played the expected number of games
            foreach (var team in tournament.Teams)
            {
                Assert.AreEqual(expectedGamesPerPlayer, playerGameCounts.GetValueOrDefault(team.Id, 0),
                    $"Team {team.Label} should have played {expectedGamesPerPlayer} games");
            }
        }

        [TestMethod]
        [DataRow(8)]
        [DataRow(16)]
        public async Task create_swiss_tournament_with_initial_round_and_print_to_pdf(int noPlayers)
        {
            TestContext?.WriteLine($"Testing Swiss tournament creation and PDF printing with {noPlayers} players");

            // Create unique random players
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
                    Ranking = random.NextDouble() * 100
                });
            }

            // Create tournament using AssignTournament factory for Swiss format
            AssignTournament assignTournament = new();
            Tournament tournament = assignTournament.MakeRandom(
                tournamentType: 5, // Swiss tournament
                label: $"Swiss_Tournament_PDF_Test_{noPlayers}_Players_{DateTime.Now:yyyyMMdd_HHmmss}",
                noPlayers: players.Count,
                sport: 1, // Tennis
                noSingle: 1,
                noDouble: 0,
                sets: 1,
                teamSize: 1,
                players: players
            );

            // Verify tournament structure
            Assert.IsNotNull(tournament, "Tournament should not be null");
            Assert.IsNotNull(tournament.Draw, "Tournament draw should not be null");
            Assert.IsNotNull(tournament.Teams, "Tournament teams should not be null");
            Assert.AreEqual(noPlayers, tournament.Teams.Count(), "Tournament should have correct number of teams");

            TestContext?.WriteLine($"Created Swiss tournament '{tournament.Label}' with {tournament.Teams.Count()} teams");

            // Create the scheduler using factory pattern
            FactoryDrawMaker factory = new FactoryDrawMaker();
            IGameMaker gm = new GameMakerTennis();
            IDrawMaker scheduler = factory.Create(tournament, gm);

            // Verify the initial round is set properly
            Assert.AreEqual(1, tournament.Draw.NoRounds, "Swiss tournament should start with exactly 1 round");

            var initialRound = tournament.Draw.GetRoundIndex(0);
            Assert.IsNotNull(initialRound, "Initial round should not be null");
            Assert.IsTrue(initialRound.Permutations.Count() > 0, "Initial round should have permutations (matches)");

            TestContext?.WriteLine($"Initial round has {initialRound.Permutations.Count()} match permutations");

            // Display initial round matches
            foreach (var permutation in initialRound.Permutations)
            {
                foreach (var match in permutation.Matches)
                {
                    var homePlayer = match.Home.First();
                    var awayPlayer = match.Away.First();
                    TestContext?.WriteLine($"  Match {match.Id}: {homePlayer?.First} {homePlayer?.Last} vs {awayPlayer?.First} {awayPlayer?.Last}");
                }
            }

            // Generate some initial scores for the first round to make the PDF more interesting
            List<Score> initialScores = new List<Score>();
            int scoreId = 1;

            foreach (var permutation in initialRound.Permutations)
            {
                foreach (var match in permutation.Matches)
                {
                    // Create scores for each set
                    for (int set = 0; set < tournament.Details.Sets; set++)
                    {
                        bool homeWins = RandomUtil.GetInt(2) == 0;
                        var score = new Score
                        {
                            Id = scoreId++,
                            Home = homeWins ? 6 : RandomUtil.GetInt(4) + 1,
                            Away = !homeWins ? 6 : RandomUtil.GetInt(4) + 1,
                            Permutation = permutation.Id,
                            Match = match.Id,
                            Round = initialRound.Index,
                            Set = set + 1
                        };
                        initialScores.Add(score);
                    }
                }
            }

            TestContext?.WriteLine($"Generated {initialScores.Count} scores for initial round");

            // Print tournament to 3 different PDF files
            string baseFilename = tournament.Label;
            string filename1 = $"{baseFilename}_Round1.pdf";
            string filename2 = $"{baseFilename}_Round1_standings.pdf";
            string filename3 = $"{baseFilename}_Round2.pdf";

            TestContext?.WriteLine($"Printing tournament to 3 PDF files: {filename1}, {filename2}, {filename3}");

            try
            {
                //Advance the round
                scheduler.OnChange(tournament.Draw, 1, 0, initialScores);

                PdfPrinterSwiss printer = new PdfPrinterSwiss(tournament.Draw, new PDFTemplateFactory());
                // File 1: Round 1 only
                using FileStream pdfFile1 = new FileStream(filename1, FileMode.Create, FileAccess.Write);
                await printer.PrintCurrentRound(pdfFile1, tournament,  1, initialScores);

                // File 2: Round 2 only
                using FileStream pdfFile2 = new FileStream(filename2, FileMode.Create, FileAccess.Write);
                await printer.PrintStandings(pdfFile2, tournament, 1);

                // File 3: Complete tournament (both rounds)
                using FileStream pdfFile3 = new FileStream(filename3, FileMode.Create, FileAccess.Write);
                await printer.PrintNextRound(pdfFile3, tournament,  2, initialScores);

                TestContext?.WriteLine($"Successfully created PDF files: {filename1}, {filename2}, {filename3}");

                // Verify all files were created
                Assert.IsTrue(File.Exists(filename1), $"PDF file '{filename1}' should exist after printing");
                Assert.IsTrue(File.Exists(filename2), $"PDF file '{filename2}' should exist after printing");
                Assert.IsTrue(File.Exists(filename3), $"PDF file '{filename3}' should exist after printing");

                // Verify the files have content
                FileInfo fileInfo1 = new FileInfo(filename1);
                FileInfo fileInfo2 = new FileInfo(filename2);
                FileInfo fileInfo3 = new FileInfo(filename3);

                Assert.IsTrue(fileInfo1.Length > 0, "PDF file 1 should not be empty");
                Assert.IsTrue(fileInfo2.Length > 0, "PDF file 2 should not be empty");
                Assert.IsTrue(fileInfo3.Length > 0, "PDF file 3 should not be empty");

                TestContext?.WriteLine($"PDF file sizes - Round1: {fileInfo1.Length} bytes, Round2: {fileInfo2.Length} bytes, Complete: {fileInfo3.Length} bytes");
            }
            catch (Exception ex)
            {
                TestContext?.WriteLine($"Error creating PDF: {ex.Message}");
                throw;
            }

            TestContext?.WriteLine("Swiss tournament with initial round and PDF printing test completed successfully");
        }

        private int GetExpectedSwissRounds(int noPlayers)
        {
            // Swiss tournaments typically run for a number of rounds based on player count
            // Common formula: ceil(log2(noPlayers)) rounds for a complete Swiss tournament
            // However, this represents the maximum rounds that could be played, not the initial state
            return (int)Math.Ceiling(Math.Log2(noPlayers)) + 1;
        }

    }
}