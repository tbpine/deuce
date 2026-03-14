using System.Diagnostics;
using deuce;

namespace deuce_unit;

[TestClass]
public class IntegrationTestsTeamStanding
{
    private const string ConnStr = "Server=localhost;Database=deuce;User Id=deuce;Password=deuce;";

    // Test-specific IDs — use high values to avoid collisions with real data
    private const int TestTournamentId = 99990;
    private const int TestTeamId1 = 99991;
    private const int TestTeamId2 = 99992;
    private const int TestTeamId3 = 99993;

    [ClassInitialize]
    public static void SetupTestData(TestContext context)
    {
        // Insert prerequisite tournament and teams using direct SQL
        // Use REPLACE to handle reruns cleanly
        DirectSQL.Run($"REPLACE INTO `tournament` (`id`, `label`, `type`, `sport`) VALUES ({TestTournamentId}, 'IntTest TeamStanding', 1, 1);");
        DirectSQL.Run($"REPLACE INTO `team` (`id`, `label`, `organization`) VALUES ({TestTeamId1}, 'IntTest Team A', 1);");
        DirectSQL.Run($"REPLACE INTO `team` (`id`, `label`, `organization`) VALUES ({TestTeamId2}, 'IntTest Team B', 1);");
        DirectSQL.Run($"REPLACE INTO `team` (`id`, `label`, `organization`) VALUES ({TestTeamId3}, 'IntTest Team C', 1);");

        // Clean up any leftover standings from previous runs
        DirectSQL.Run($"DELETE FROM `team_standing` WHERE `tournament` = {TestTournamentId};");
    }

    [ClassCleanup]
    public static void CleanupTestData()
    {
        DirectSQL.Run($"DELETE FROM `team_standing` WHERE `tournament` = {TestTournamentId};");
        DirectSQL.Run($"DELETE FROM `tournament` WHERE `id` = {TestTournamentId};");
        DirectSQL.Run($"DELETE FROM `team` WHERE `id` IN ({TestTeamId1}, {TestTeamId2}, {TestTeamId3});");
    }

    [TestCleanup]
    public void CleanupStandingsBetweenTests()
    {
        // Ensure each test starts with a clean slate for standings
        DirectSQL.Run($"DELETE FROM `team_standing` WHERE `tournament` = {TestTournamentId};");
    }

    private static TeamStanding MakeStanding(int teamId, int wins, int losses, int draws, double points, int position)
    {
        return new TeamStanding
        {
            TeamId = teamId,
            Tournament = TestTournamentId,
            Wins = wins,
            Losses = losses,
            Draws = draws,
            Points = points,
            Position = position
        };
    }

    [TestMethod]
    public void insert_standing_assigns_id()
    {
        DbConnectionLocal conn = new(ConnStr);
        DbRepoTeamStanding repo = new(conn);

        var standing = MakeStanding(TestTeamId1, wins: 5, losses: 2, draws: 1, points: 16, position: 1);
        repo.Set(standing);

        Assert.IsTrue(standing.Id > 0, "Insert should assign an auto-generated ID");
        Debug.WriteLine($"Inserted standing with ID: {standing.Id}");
    }

    [TestMethod]
    public async Task insert_and_retrieve_standing()
    {
        DbConnectionLocal conn = new(ConnStr);
        DbRepoTeamStanding repo = new(conn);

        var standing = MakeStanding(TestTeamId1, wins: 7, losses: 1, draws: 0, points: 21, position: 1);
        repo.Set(standing);

        // Retrieve
        var results = await repo.GetStandingsForTournament(TestTournamentId);

        Assert.AreEqual(1, results.Count, "Should retrieve exactly 1 standing");
        var retrieved = results[0];
        Assert.AreEqual(standing.Id, retrieved.Id);
        Assert.AreEqual(TestTeamId1, retrieved.TeamId);
        Assert.AreEqual(TestTournamentId, retrieved.Tournament);
        Assert.AreEqual(7, retrieved.Wins);
        Assert.AreEqual(1, retrieved.Losses);
        Assert.AreEqual(0, retrieved.Draws);
        Assert.AreEqual(21, retrieved.Points);
        Assert.AreEqual(1, retrieved.Position);
    }

    [TestMethod]
    public async Task insert_multiple_standings_and_retrieve_ordered_by_position()
    {
        DbConnectionLocal conn = new(ConnStr);
        DbRepoTeamStanding repo = new(conn);

        var s1 = MakeStanding(TestTeamId1, wins: 5, losses: 2, draws: 1, points: 16, position: 2);
        var s2 = MakeStanding(TestTeamId2, wins: 7, losses: 1, draws: 0, points: 21, position: 1);
        var s3 = MakeStanding(TestTeamId3, wins: 3, losses: 4, draws: 1, points: 10, position: 3);

        repo.Set(s1);
        repo.Set(s2);
        repo.Set(s3);

        var results = await repo.GetStandingsForTournament(TestTournamentId);

        Assert.AreEqual(3, results.Count);
        // sp_get_team_standing orders by position
        Assert.AreEqual(TestTeamId2, results[0].TeamId, "Position 1 should be first");
        Assert.AreEqual(TestTeamId1, results[1].TeamId, "Position 2 should be second");
        Assert.AreEqual(TestTeamId3, results[2].TeamId, "Position 3 should be third");
    }

    [TestMethod]
    public async Task update_standing_persists_changes()
    {
        DbConnectionLocal conn = new(ConnStr);
        DbRepoTeamStanding repo = new(conn);

        var standing = MakeStanding(TestTeamId1, wins: 3, losses: 3, draws: 2, points: 11, position: 1);
        repo.Set(standing);
        int originalId = standing.Id;

        // Update values
        standing.Wins = 6;
        standing.Losses = 1;
        standing.Draws = 1;
        standing.Points = 19;
        repo.Set(standing);

        var results = await repo.GetStandingsForTournament(TestTournamentId);

        Assert.AreEqual(1, results.Count, "Update should not create a duplicate row");
        var retrieved = results[0];
        Assert.AreEqual(originalId, retrieved.Id, "ID should remain the same after update");
        Assert.AreEqual(6, retrieved.Wins);
        Assert.AreEqual(1, retrieved.Losses);
        Assert.AreEqual(1, retrieved.Draws);
        Assert.AreEqual(19, retrieved.Points);
    }

    [TestMethod]
    public async Task delete_standing_removes_from_database()
    {
        DbConnectionLocal conn = new(ConnStr);
        DbRepoTeamStanding repo = new(conn);

        var standing = MakeStanding(TestTeamId1, wins: 4, losses: 4, draws: 0, points: 12, position: 1);
        repo.Set(standing);
        Assert.IsTrue(standing.Id > 0);

        repo.Delete(standing);

        var results = await repo.GetStandingsForTournament(TestTournamentId);
        Assert.AreEqual(0, results.Count, "Standing should be removed after delete");
    }

    [TestMethod]
    public async Task get_standings_for_empty_tournament_returns_empty_list()
    {
        DbConnectionLocal conn = new(ConnStr);
        DbRepoTeamStanding repo = new(conn);

        var results = await repo.GetStandingsForTournament(TestTournamentId);

        Assert.IsNotNull(results);
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public async Task get_standings_includes_team_name()
    {
        DbConnectionLocal conn = new(ConnStr);
        DbRepoTeamStanding repo = new(conn);

        var standing = MakeStanding(TestTeamId1, wins: 2, losses: 5, draws: 1, points: 7, position: 1);
        repo.Set(standing);

        var results = await repo.GetStandingsForTournament(TestTournamentId);

        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("IntTest Team A", results[0].Team.Label, "Should join team name from team table");
    }

    [TestMethod]
    public async Task sync_adds_new_standings()
    {
        DbConnectionLocal conn = new(ConnStr);
        conn.KeepAlive(true);
        DbRepoTeamStanding repo = new(conn);

        var standings = new List<TeamStanding>
        {
            MakeStanding(TestTeamId1, wins: 3, losses: 1, draws: 0, points: 9, position: 1),
            MakeStanding(TestTeamId2, wins: 2, losses: 2, draws: 0, points: 6, position: 2)
        };

        await repo.Sync(standings);

        conn.KeepAlive(false);
        var results = await repo.GetStandingsForTournament(TestTournamentId);

        Assert.AreEqual(2, results.Count);
        Assert.IsTrue(standings[0].Id > 0, "Sync should assign IDs to new records");
        Assert.IsTrue(standings[1].Id > 0, "Sync should assign IDs to new records");
    }

    [TestMethod]
    public async Task sync_updates_existing_standings()
    {
        DbConnectionLocal conn = new(ConnStr);
        conn.KeepAlive(true);
        DbRepoTeamStanding repo = new(conn);

        // Initial sync
        var initial = new List<TeamStanding>
        {
            MakeStanding(TestTeamId1, wins: 1, losses: 0, draws: 0, points: 3, position: 1),
            MakeStanding(TestTeamId2, wins: 0, losses: 1, draws: 0, points: 0, position: 2)
        };
        await repo.Sync(initial);

        // Updated values — same teams, new stats
        var updated = new List<TeamStanding>
        {
            MakeStanding(TestTeamId1, wins: 2, losses: 1, draws: 0, points: 6, position: 1),
            MakeStanding(TestTeamId2, wins: 1, losses: 2, draws: 0, points: 3, position: 2)
        };
        await repo.Sync(updated);

        conn.KeepAlive(false);
        var results = await repo.GetStandingsForTournament(TestTournamentId);

        Assert.AreEqual(2, results.Count, "Sync should not create duplicate records");
        var team1 = results.First(r => r.TeamId == TestTeamId1);
        Assert.AreEqual(2, team1.Wins, "Wins should be updated");
        Assert.AreEqual(6, team1.Points, "Points should be updated");
    }

    [TestMethod]
    public async Task sync_removes_standings_no_longer_in_source()
    {
        DbConnectionLocal conn = new(ConnStr);
        conn.KeepAlive(true);
        DbRepoTeamStanding repo = new(conn);

        // Initial sync with 3 teams
        var initial = new List<TeamStanding>
        {
            MakeStanding(TestTeamId1, wins: 1, losses: 0, draws: 0, points: 3, position: 1),
            MakeStanding(TestTeamId2, wins: 0, losses: 1, draws: 0, points: 0, position: 2),
            MakeStanding(TestTeamId3, wins: 0, losses: 0, draws: 1, points: 1, position: 3)
        };
        await repo.Sync(initial);

        // Second sync with only 2 teams — team 3 should be removed
        var reduced = new List<TeamStanding>
        {
            MakeStanding(TestTeamId1, wins: 2, losses: 0, draws: 0, points: 6, position: 1),
            MakeStanding(TestTeamId2, wins: 0, losses: 2, draws: 0, points: 0, position: 2)
        };
        await repo.Sync(reduced);

        conn.KeepAlive(false);
        var results = await repo.GetStandingsForTournament(TestTournamentId);

        Assert.AreEqual(2, results.Count, "Third standing should be removed");
        Assert.IsFalse(results.Any(r => r.TeamId == TestTeamId3), "Team C should no longer be in standings");
    }

    [TestMethod]
    public async Task sync_handles_add_update_and_delete_together()
    {
        DbConnectionLocal conn = new(ConnStr);
        conn.KeepAlive(true);
        DbRepoTeamStanding repo = new(conn);

        // Initial: Team A and Team B
        var initial = new List<TeamStanding>
        {
            MakeStanding(TestTeamId1, wins: 1, losses: 0, draws: 0, points: 3, position: 1),
            MakeStanding(TestTeamId2, wins: 0, losses: 1, draws: 0, points: 0, position: 2)
        };
        await repo.Sync(initial);

        // Second sync: keep Team A (updated), remove Team B, add Team C
        var mixed = new List<TeamStanding>
        {
            MakeStanding(TestTeamId1, wins: 2, losses: 0, draws: 0, points: 6, position: 1),
            MakeStanding(TestTeamId3, wins: 1, losses: 1, draws: 0, points: 3, position: 2)
        };
        await repo.Sync(mixed);

        conn.KeepAlive(false);
        var results = await repo.GetStandingsForTournament(TestTournamentId);

        Assert.AreEqual(2, results.Count);
        Assert.IsTrue(results.Any(r => r.TeamId == TestTeamId1), "Team A should still exist");
        Assert.IsTrue(results.Any(r => r.TeamId == TestTeamId3), "Team C should be added");
        Assert.IsFalse(results.Any(r => r.TeamId == TestTeamId2), "Team B should be removed");

        var teamA = results.First(r => r.TeamId == TestTeamId1);
        Assert.AreEqual(2, teamA.Wins, "Team A wins should be updated");
    }
}
