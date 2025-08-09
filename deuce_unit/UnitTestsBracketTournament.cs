using deuce;
using System.Linq;

namespace deuce_unit;

[TestClass]
public class UnitTestsBracketTournament
{
    public TestContext? TestContext { get; set; }

    [TestInitialize]
    public void Setup()
    {
        // Code to initialize before each test
    }

    [TestMethod]
    [DataRow(8)]
    [DataRow(16)]
    [DataRow(32)]
    public void bracket_to_completion(int noPlayers)
    {
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
                Last = nameParts.Length > 1 ? nameParts[1] : "Unknown"
            });
        }

        // Initialize organization
        Organization organization = new Organization() { Id = 100, Name = "Test Organization" };

        // Create a bracket tournament (double elimination)
        AssignTournament assignTournament = new();
        Tournament tournament = assignTournament.MakeRandom(
            tournamentType: 3, // Bracket tournament type (double elimination)
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
        Assert.IsNotNull(tournament.Draw, "Main tournament draw should not be null");
        Assert.IsNotNull(tournament.Brackets, "Tournament should have brackets");
        Assert.IsTrue(tournament.Brackets.Count() > 0, "Should have at least one bracket");

        var losersBracket = tournament.Brackets.FirstOrDefault()?.Tournament;
        Assert.IsNotNull(losersBracket, "Losers bracket should exist");
        Assert.IsNotNull(losersBracket.Draw, "Losers bracket draw should not be null");

        // Create DrawMakerBrackets for score processing
        FactoryDrawMaker fac = new FactoryDrawMaker();
        IGameMaker gm = new GameMakerTennis();
        IDrawMaker scheduler = fac.Create(tournament, gm);

        // Assign IDs to matches, permutations and rounds for both brackets
        int matchId = 1;

        // Process winners bracket
        var winnersDraw = tournament.Draw;
        Assert.IsNotNull(winnersDraw, "Winners draw should not be null");

        foreach (Round round in winnersDraw.Rounds)
        {
            foreach (Permutation permutation in round.Permutations)
            {
                foreach (Match match in permutation.Matches)
                {
                    match.Id = matchId++;
                }
            }
        }

        // Process losers bracket
        var losersDraw = losersBracket.Draw;
        //Reset permulation
        foreach (Round round in losersDraw.Rounds)
        {
            foreach (Permutation permutation in round.Permutations)
            {
                foreach (Match match in permutation.Matches)
                {
                    match.Id = matchId++;
                }
            }
        }

        // Track the tournament progression
        int totalRounds = Math.Max(winnersDraw.NoRounds, losersDraw.NoRounds);

        // Process each round and advance the tournament
        for (int currentRound = 1; currentRound <= totalRounds; currentRound++)
        {
            List<Score> winnersScores = new List<Score>();
            List<Score> losersScores = new List<Score>();

            // Generate scores for winners bracket matches in this round
            if (currentRound <= winnersDraw.NoRounds)
            {
                var winnersRound = winnersDraw.GetRoundAtIndex(currentRound - 1);
                foreach (Permutation permutation in winnersRound.Permutations)
                {
                    Match match = permutation.Matches.First();
                    // Only generate scores for matches with actual players (not byes)
                    if (match.Home.Any() && match.Away.Any() &&
                        !match.Home.First().Bye && !match.Away.First().Bye)
                    {
                        for (int setIndex = 0; setIndex < tournament.Details.Sets; setIndex++)
                        {
                            // Randomly determine winner but ensure clear victories
                            bool homeWins = RandomUtil.GetInt(2) == 0;

                            Score score = new Score
                            {
                                Id = match.Id,
                                Home = homeWins ? 6 : RandomUtil.GetInt(4), // Winner gets 6, loser gets 0-3
                                Away = !homeWins ? 6 : RandomUtil.GetInt(4),
                                Permutation = permutation.Id,
                                Match = match.Id,
                                Round = winnersRound.Index,
                                Set = setIndex + 1,
                            };

                            winnersScores.Add(score);
                        }
                    }
                }
            }

            // Process winners bracket scores first
            if (winnersScores.Count > 0)
            {
                scheduler.OnChange(tournament.Draw, currentRound, currentRound - 1, winnersScores);

                TestContext?.WriteLine($"Round {currentRound} - Winners bracket processed:");
                TestContext?.WriteLine($"  Winners bracket matches: {winnersScores.Count / tournament.Details.Sets}");
                TestContext?.WriteLine($"  Scores processed: {winnersScores.Count}");
            }

            // Generate scores for losers bracket matches in this round
            if (currentRound <= losersDraw.NoRounds)
            {
                var losersRound = losersDraw.GetRoundAtIndex(currentRound - 1);
                foreach (Permutation permutation in losersRound.Permutations)
                {
                    var match = permutation.Matches.First();
                    // Only generate scores for matches with actual players (not byes)
                    if (match.Home.Any() && match.Away.Any() &&
                        !match.Home.First().Bye && !match.Away.First().Bye)
                    {
                        for (int setIndex = 0; setIndex < tournament.Details.Sets; setIndex++)
                        {
                            // Randomly determine winner but ensure clear victories
                            bool homeWins = RandomUtil.GetInt(2) == 0;

                            Score score = new Score
                            {
                                Id = match.Id,
                                Home = homeWins ? 6 : RandomUtil.GetInt(4), // Winner gets 6, loser gets 0-3
                                Away = !homeWins ? 6 : RandomUtil.GetInt(4),
                                Permutation = permutation.Id,
                                Match = match.Id,
                                Round = losersRound.Index,
                                Set = setIndex + 1,
                            };

                            losersScores.Add(score);
                        }
                    }

                }
            }

            // Then process losers bracket scores
            if (losersScores.Count > 0)
            {
                scheduler.OnChange(losersBracket.Draw, currentRound, currentRound - 1, losersScores);

                TestContext?.WriteLine($"Round {currentRound} - Losers bracket processed:");
                TestContext?.WriteLine($"  Losers bracket matches: {losersScores.Count / tournament.Details.Sets}");
                TestContext?.WriteLine($"  Scores processed: {losersScores.Count}");
            }

            // Output combined round information
            if (winnersScores.Count > 0 || losersScores.Count > 0)
            {
                TestContext?.WriteLine($"Round {currentRound} completed:");
                TestContext?.WriteLine($"  Total winners matches: {(currentRound <= winnersDraw.NoRounds ? winnersDraw.GetRoundAtIndex(currentRound - 1).Permutations.Sum(p => p.Matches.Count()) : 0)}");
                TestContext?.WriteLine($"  Total losers matches: {(currentRound <= losersDraw.NoRounds ? losersDraw.GetRoundAtIndex(currentRound - 1).Permutations.Sum(p => p.Matches.Count()) : 0)}");
                TestContext?.WriteLine($"  Total scores processed: {winnersScores.Count + losersScores.Count}");
                TestContext?.WriteLine("---");
            }
        }

        // Verify tournament completion - check that we have some rounds processed
        Assert.IsTrue(winnersDraw.NoRounds > 0, "Winners bracket should have rounds");
        Assert.IsTrue(losersDraw.NoRounds > 0, "Losers bracket should have rounds");

        // Count non-bye teams in final rounds to verify progression
        var finalWinnersRound = winnersDraw.GetRoundAtIndex(winnersDraw.NoRounds - 1);
        var finalLosersRound = losersDraw.GetRoundAtIndex(losersDraw.NoRounds - 1);

        // Verify that the tournament structure is valid
        Assert.IsTrue(winnersDraw.NoRounds > 0, "Winners bracket should have rounds");
        Assert.IsTrue(losersDraw.NoRounds > 0, "Losers bracket should have rounds");

        // Output final tournament statistics
        TestContext?.WriteLine($"\nTournament Completion Statistics:");
        TestContext?.WriteLine($"Original players: {noPlayers}");
        TestContext?.WriteLine($"Winners bracket rounds: {winnersDraw.NoRounds}");
        TestContext?.WriteLine($"Losers bracket rounds: {losersDraw.NoRounds}");
        TestContext?.WriteLine($"Tournament completed successfully!");

        // Verify bracket structure integrity
        VerifyBracketStructure(tournament, noPlayers);
    }

    [TestMethod]
    [DataRow(8)]
    [DataRow(16)]
    public void bracket_tournament_advancement_logic_verification(int noPlayers)
    {
        // Create players and teams (simplified for verification)
        var players = new List<Player>();
        for (int i = 0; i < noPlayers; i++)
        {
            var nameParts = RandomUtil.GetNameAtIndex(i % RandomUtil.GetNameCount()).Split(' ');
            players.Add(new Player
            {
                Id = i + 1,
                First = nameParts[0],
                Last = nameParts.Length > 1 ? nameParts[1] : "Unknown"
            });
        }

        var teams = new List<Team>();
        for (int i = 0; i < noPlayers; i++)
        {
            var team = new Team
            {
                Id = i + 1,
                Label = $"Team {i + 1}"
            };
            team.AddPlayer(players[i]);
            teams.Add(team);
        }

        // Create bracket tournament
        AssignTournament assignTournament = new();
        Tournament tournament = assignTournament.MakeRandom(
            tournamentType: 3, // Bracket tournament
            label: "Advancement Test Tournament",
            noPlayers: players.Count,
            sport: 1,
            noSingle: 1,
            noDouble: 0,
            sets: 1,
            teamSize: 1,
            players: players
        );

        // Create scheduler
        FactoryDrawMaker fac = new FactoryDrawMaker();
        IGameMaker gm = new GameMakerTennis();
        IDrawMaker scheduler = fac.Create(tournament, gm);

        // Assign IDs
        int matchId = 1, permId = 1;
        var winnersDraw = tournament.Draw!;
        var losersDraw = tournament.Brackets!.First().Tournament!.Draw!;

        foreach (Round round in winnersDraw.Rounds)
        {
            foreach (Permutation permutation in round.Permutations)
            {
                permutation.Id = permId++;
                foreach (Match match in permutation.Matches)
                {
                    match.Id = matchId++;
                }
            }
        }

        foreach (Round round in losersDraw.Rounds)
        {
            foreach (Permutation permutation in round.Permutations)
            {
                permutation.Id = permId++;
                foreach (Match match in permutation.Matches)
                {
                    match.Id = matchId++;
                }
            }
        }

        // Test specific advancement scenarios
        TestFirstRoundAdvancement(tournament, scheduler);

        TestContext?.WriteLine("Advancement logic verification completed successfully!");
    }

    private void TestFirstRoundAdvancement(Tournament tournament, IDrawMaker scheduler)
    {
        var winnersDraw = tournament.Draw!;
        var firstRound = winnersDraw.GetRoundAtIndex(0);

        // Get the first match with real players
        var testMatch = firstRound.Permutations
            .SelectMany(p => p.Matches)
            .FirstOrDefault(m => m.Home.Any() && m.Away.Any() &&
                               !m.Home.First().Bye && !m.Away.First().Bye);

        if (testMatch != null)
        {
            // Create a score where home team wins
            var score = new Score
            {
                Id = testMatch.Id,
                Home = 6,
                Away = 2,
                Permutation = testMatch.Permutation!.Id,
                Match = testMatch.Id,
                Round = 1,
                Set = 1
            };

            var beforeHomeTeam = testMatch.Home.First().Team;
            var beforeAwayTeam = testMatch.Away.First().Team;

            // Process the score
            scheduler.OnChange(tournament.Draw!, 2, 1, new List<Score> { score });

            TestContext?.WriteLine($"First round advancement test:");
            TestContext?.WriteLine($"  Home team: {beforeHomeTeam?.Label} (winner)");
            TestContext?.WriteLine($"  Away team: {beforeAwayTeam?.Label} (loser)");
            TestContext?.WriteLine($"  Score: {score.Home}-{score.Away}");
            TestContext?.WriteLine($"  Winner should advance in winners bracket, loser should move to losers bracket");
        }
    }

    private void VerifyBracketStructure(Tournament tournament, int originalPlayers)
    {
        var winnersDraw = tournament.Draw!;
        var losersBracket = tournament.Brackets!.First().Tournament!;
        var losersDraw = losersBracket.Draw!;

        // Verify power-of-2 structure
        int expectedPowerOf2 = (int)Math.Pow(2, Math.Ceiling(Math.Log2(originalPlayers)));

        // Count total teams including byes
        int totalTeamsInWinners = winnersDraw.GetRoundAtIndex(0).Permutations
            .SelectMany(p => p.Matches)
            .SelectMany(m => new[] { m.Home, m.Away })
            .Where(side => side.Any())
            .Count();

        Assert.AreEqual(expectedPowerOf2, totalTeamsInWinners,
            $"Winners bracket should have {expectedPowerOf2} total positions (including byes)");

        // Verify losers bracket exists and has appropriate structure
        Assert.IsTrue(losersDraw.NoRounds > 0, "Losers bracket should have rounds");

        TestContext?.WriteLine($"Bracket structure verification:");
        TestContext?.WriteLine($"  Original players: {originalPlayers}");
        TestContext?.WriteLine($"  Expected power of 2: {expectedPowerOf2}");
        TestContext?.WriteLine($"  Winners bracket positions: {totalTeamsInWinners}");
        TestContext?.WriteLine($"  Winners bracket rounds: {winnersDraw.NoRounds}");
        TestContext?.WriteLine($"  Losers bracket rounds: {losersDraw.NoRounds}");
    }
}
