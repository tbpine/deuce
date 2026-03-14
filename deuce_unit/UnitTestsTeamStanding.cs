using deuce;

namespace deuce_unit;

[TestClass]
public class UnitTestsTeamStanding
{
    public TestContext? TestContext { get; set; }

    private static readonly Random _random = new();

    private List<TeamStanding> GenerateRandomStandings(int count, int tournamentId)
    {
        var standings = new List<TeamStanding>();
        var players = RandomUtil.GetRandomPlayers(count);

        for (int i = 0; i < count; i++)
        {
            int wins = _random.Next(0, 10);
            int losses = _random.Next(0, 10);
            int draws = _random.Next(0, 5);

            standings.Add(new TeamStanding
            {
                Id = i + 1,
                TeamId = players[i].Id,
                Tournament = tournamentId,
                Team = new Team { Id = players[i].Id, Label = $"{players[i].First} {players[i].Last}" },
                Wins = wins,
                Losses = losses,
                Draws = draws,
                Points = (wins * 3) + draws,
                Position = i + 1
            });
        }

        return standings;
    }

    [TestMethod]
    [DataRow(4)]
    [DataRow(8)]
    [DataRow(16)]
    public void set_standings_for_round_stores_correctly(int teamCount)
    {
        var tournament = new Tournament { Id = 1 };
        var standings = GenerateRandomStandings(teamCount, tournament.Id);

        tournament.SetStandingsForRound(0, standings);

        var retrieved = tournament.GetStandingsForRound(0);
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(teamCount, retrieved.Count);

        for (int i = 0; i < teamCount; i++)
        {
            Assert.AreEqual(standings[i].TeamId, retrieved[i].TeamId);
            Assert.AreEqual(standings[i].Wins, retrieved[i].Wins);
            Assert.AreEqual(standings[i].Losses, retrieved[i].Losses);
            Assert.AreEqual(standings[i].Draws, retrieved[i].Draws);
            Assert.AreEqual(standings[i].Points, retrieved[i].Points);
            Assert.AreEqual(standings[i].Position, retrieved[i].Position);
        }
    }

    [TestMethod]
    public void get_standings_for_nonexistent_round_returns_null()
    {
        var tournament = new Tournament { Id = 1 };

        var result = tournament.GetStandingsForRound(99);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void get_current_standings_returns_empty_when_no_rounds()
    {
        var tournament = new Tournament { Id = 1 };

        var current = tournament.GetCurrentStandings();

        Assert.IsNotNull(current);
        Assert.AreEqual(0, current.Count);
    }

    [TestMethod]
    [DataRow(3)]
    [DataRow(5)]
    public void get_current_standings_returns_latest_round(int totalRounds)
    {
        var tournament = new Tournament { Id = 1 };

        for (int round = 0; round < totalRounds; round++)
        {
            var standings = GenerateRandomStandings(8, tournament.Id);
            tournament.SetStandingsForRound(round, standings);
        }

        var current = tournament.GetCurrentStandings();
        var lastRound = tournament.GetStandingsForRound(totalRounds - 1);

        Assert.IsNotNull(current);
        Assert.IsNotNull(lastRound);
        Assert.AreEqual(lastRound.Count, current.Count);

        for (int i = 0; i < current.Count; i++)
        {
            Assert.AreEqual(lastRound[i].TeamId, current[i].TeamId);
            Assert.AreEqual(lastRound[i].Points, current[i].Points);
        }
    }

    [TestMethod]
    public void get_all_standings_returns_all_rounds()
    {
        var tournament = new Tournament { Id = 1 };
        int totalRounds = 4;

        for (int round = 0; round < totalRounds; round++)
        {
            tournament.SetStandingsForRound(round, GenerateRandomStandings(8, tournament.Id));
        }

        var allStandings = tournament.GetAllStandings();

        Assert.AreEqual(totalRounds, allStandings.Count);
        for (int round = 0; round < totalRounds; round++)
        {
            Assert.IsTrue(allStandings.ContainsKey(round));
            Assert.AreEqual(8, allStandings[round].Count);
        }
    }

    [TestMethod]
    public void clear_standings_removes_all_rounds()
    {
        var tournament = new Tournament { Id = 1 };
        tournament.SetStandingsForRound(0, GenerateRandomStandings(8, tournament.Id));
        tournament.SetStandingsForRound(1, GenerateRandomStandings(8, tournament.Id));

        tournament.ClearStandings();

        Assert.AreEqual(0, tournament.GetCurrentStandings().Count);
        Assert.AreEqual(0, tournament.GetAllStandings().Count);
    }

    [TestMethod]
    public void set_standings_for_round_creates_deep_copy()
    {
        var tournament = new Tournament { Id = 1 };
        var standings = GenerateRandomStandings(4, tournament.Id);

        tournament.SetStandingsForRound(0, standings);

        // Modify the original list
        standings[0].Wins = 999;
        standings[0].Points = 9999;

        var retrieved = tournament.GetStandingsForRound(0);
        Assert.IsNotNull(retrieved);
        Assert.AreNotEqual(999, retrieved[0].Wins, "Standings should be a deep copy");
        Assert.AreNotEqual(9999, retrieved[0].Points, "Standings should be a deep copy");
    }

    [TestMethod]
    public void standings_property_throws_on_set()
    {
        var tournament = new Tournament { Id = 1 };

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            tournament.Standings = new List<TeamStanding>();
        });
    }

    [TestMethod]
    public void overwrite_standings_for_existing_round()
    {
        var tournament = new Tournament { Id = 1 };
        var original = GenerateRandomStandings(8, tournament.Id);
        tournament.SetStandingsForRound(0, original);

        var updated = GenerateRandomStandings(8, tournament.Id);
        tournament.SetStandingsForRound(0, updated);

        var retrieved = tournament.GetStandingsForRound(0);
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(8, retrieved.Count);

        // Should reflect the updated standings, not the original
        for (int i = 0; i < 8; i++)
        {
            Assert.AreEqual(updated[i].Wins, retrieved[i].Wins);
            Assert.AreEqual(updated[i].Points, retrieved[i].Points);
        }
    }

    [TestMethod]
    public void sync_master_identifies_adds_updates_and_deletes()
    {
        var source = GenerateRandomStandings(6, 1);
        var dest = GenerateRandomStandings(6, 1);

        // Give some overlap: dest items 0-3 share IDs with source 0-3
        for (int i = 0; i < 4; i++)
        {
            dest[i].Id = source[i].Id;
        }
        // dest items 4-5 have unique IDs (will be removed)
        dest[4].Id = 100;
        dest[5].Id = 101;
        // source items 4-5 have unique IDs (will be added)
        source[4].Id = 200;
        source[5].Id = 201;

        var addList = new List<TeamStanding>();
        var updateList = new List<SyncMasterArgs<TeamStanding>>();
        var removeList = new List<TeamStanding>();

        var sync = new SyncMaster<TeamStanding>(source, dest);
        sync.Add += (s, e) => addList.Add(e);
        sync.Update += (s, e) => updateList.Add(e);
        sync.Remove += (s, e) => removeList.Add(e);

        sync.Run();

        Assert.AreEqual(2, addList.Count, "Should add 2 new standings");
        Assert.AreEqual(4, updateList.Count, "Should update 4 existing standings");
        Assert.AreEqual(2, removeList.Count, "Should remove 2 old standings");

        // Verify the correct IDs were added/removed
        Assert.IsTrue(addList.Any(s => s.Id == 200));
        Assert.IsTrue(addList.Any(s => s.Id == 201));
        Assert.IsTrue(removeList.Any(s => s.Id == 100));
        Assert.IsTrue(removeList.Any(s => s.Id == 101));
    }

    [TestMethod]
    [DataRow(4)]
    [DataRow(8)]
    [DataRow(16)]
    public void standings_points_calculation_is_consistent(int teamCount)
    {
        var standings = GenerateRandomStandings(teamCount, 1);

        foreach (var standing in standings)
        {
            double expectedPoints = (standing.Wins * 3) + standing.Draws;
            Assert.AreEqual(expectedPoints, standing.Points,
                $"Team {standing.TeamId} points should equal (wins*3)+draws");
        }
    }

    [TestMethod]
    public void standings_positions_are_unique_within_round()
    {
        var tournament = new Tournament { Id = 1 };
        var standings = GenerateRandomStandings(8, tournament.Id);
        tournament.SetStandingsForRound(0, standings);

        var retrieved = tournament.GetStandingsForRound(0);
        Assert.IsNotNull(retrieved);

        var positions = retrieved.Select(s => s.Position).ToList();
        Assert.AreEqual(positions.Count, positions.Distinct().Count(),
            "All positions within a round should be unique");
    }

    [TestMethod]
    public void multiple_rounds_standings_are_independent()
    {
        var tournament = new Tournament { Id = 1 };
        var round0 = GenerateRandomStandings(8, tournament.Id);
        var round1 = GenerateRandomStandings(8, tournament.Id);

        tournament.SetStandingsForRound(0, round0);
        tournament.SetStandingsForRound(1, round1);

        var r0 = tournament.GetStandingsForRound(0);
        var r1 = tournament.GetStandingsForRound(1);

        Assert.IsNotNull(r0);
        Assert.IsNotNull(r1);

        // Modifying round 1 standings should not affect round 0
        for (int i = 0; i < 8; i++)
        {
            Assert.AreEqual(round0[i].Wins, r0[i].Wins);
            Assert.AreEqual(round1[i].Wins, r1[i].Wins);
        }
    }
}
