using System.Data.Common;
using deuce.ext;

namespace deuce;

/// <summary>
/// Repository class for managing TeamStanding entities in the database.
/// </summary>
public class DbRepoTeamStanding : DbRepoBase<TeamStanding>
{
    public DbRepoTeamStanding(DbConnection dbconn) : base(dbconn)
    {
    }

    /// <summary>
    /// Inserts or updates a team standing record in the database.
    /// </summary>
    /// <param name="obj">
    /// The <see cref="TeamStanding"/> object containing the standing details to be stored.
    /// If the <c>Id</c> property of the object is less than 1, a new record will be inserted.
    /// Otherwise, the existing record with the specified <c>Id</c> will be updated.
    /// </param>
    /// <remarks>
    /// This method uses a stored procedure named "sp_set_team_standing" to perform the database operation.
    /// The database connection is opened before executing the command and closed afterward.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the database connection cannot be opened or the stored procedure execution fails.
    /// </exception>
    public override void Set(TeamStanding obj)
    {
        _dbconn.Open();
        //Explicitly insert new rows if id < 1
        object primaryKeyId = obj.Id < 1 ? DBNull.Value : obj.Id;
        var localTran = _dbconn.BeginTransaction();
        try
        {
            var command = _dbconn.CreateCommandStoreProc("sp_set_team_standing", 
                ["p_id", "p_team", "p_tournament", "p_wins", "p_losses", "p_draws", "p_points", "p_position"], 
                [primaryKeyId, obj.TeamId, obj.Tournament, obj.Wins, obj.Losses, obj.Draws, obj.Points, obj.Position], 
                localTran);
            
            object? result = command.ExecuteScalar();
            if (primaryKeyId == DBNull.Value)
                obj.Id = command.GetIntegerFromScaler(result);

            localTran.Commit();
        }
        catch (DbException ex)
        {
            localTran.Rollback();
            throw new InvalidOperationException("Error executing stored procedure.", ex);
        }

        _dbconn.Close();
    }

    /// <summary>
    /// Retrieves a list of team standings from the database based on the provided filter criteria.
    /// </summary>
    /// <param name="filter">The filter criteria for retrieving team standings.</param>
    /// <returns>A list of TeamStanding objects.</returns>
    /// <remarks>
    /// This method uses a stored procedure named "sp_get_team_standing" to retrieve the data.
    /// The filter parameter is used to specify the tournament ID for filtering the results.
    /// </remarks>
    public override async Task<List<TeamStanding>> GetList(Filter filter)
    {
        var standings = new List<TeamStanding>();
        _dbconn.Open();
        var command = _dbconn.CreateCommandStoreProc("sp_get_team_standing",
            new[] { "p_tournament" },
            new object[] { filter.TournamentId });

        // Execute the command asynchronously and read the results
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var standing = new TeamStanding
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                TeamId = reader.GetInt32(reader.GetOrdinal("team")),
                Tournament = reader.GetInt32(reader.GetOrdinal("tournament")),
                Wins = reader.GetInt32(reader.GetOrdinal("wins")),
                Losses = reader.GetInt32(reader.GetOrdinal("losses")),
                Draws = reader.GetInt32(reader.GetOrdinal("draws")),
                Points = reader.GetDouble(reader.GetOrdinal("points")),
                Position = reader.GetInt32(reader.GetOrdinal("position")),
                Team = new Team
                {
                    Id = reader.GetInt32(reader.GetOrdinal("team")),
                    Label = reader.IsDBNull(reader.GetOrdinal("team_name")) 
                        ? "" 
                        : reader.GetString(reader.GetOrdinal("team_name"))
                }
            };
            standings.Add(standing);
        }
        _dbconn.Close();
        return standings;
    }

    /// <summary>
    /// Deletes a team standing record from the database.
    /// </summary>
    /// <param name="obj">The TeamStanding object to delete.</param>
    /// <remarks>
    /// This method uses a stored procedure named "sp_delete_team_standing" to delete the record.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when the obj parameter is null.</exception>
    public override void Delete(TeamStanding obj)
    {
        TeamStanding standing = obj as TeamStanding ?? throw new ArgumentNullException(nameof(obj));

        _dbconn.Open();
        var localTran = _dbconn.BeginTransaction();
        var command = _dbconn.CreateCommandStoreProc("sp_delete_team_standing", ["p_id"],
            [standing.Id], localTran);
        try
        {
            command.ExecuteNonQuery();
            localTran.Commit();
        }
        catch (DbException)
        {
            localTran.Rollback();
        }
        _dbconn.Close();
    }

    /// <summary>
    /// Retrieves team standings for a specific tournament.
    /// </summary>
    /// <param name="tournamentId">The tournament ID to filter standings.</param>
    /// <returns>A list of TeamStanding objects for the specified tournament.</returns>
    public async Task<List<TeamStanding>> GetStandingsForTournament(int tournamentId)
    {
        var filter = new Filter { TournamentId = tournamentId };
        return await GetList(filter);
    }

    /// <summary>
    /// Synchronizes a list of team standings with the database.
    /// This method compares the source list with existing database records
    /// and adds, updates, or removes records as necessary.
    /// </summary>
    /// <param name="src">The source list of team standings to synchronize.</param>
    /// <returns>A task representing the asynchronous synchronization operation.</returns>
    public override async Task Sync(List<TeamStanding> src)
    {
        if (src == null || src.Count == 0)
            return;

        _dbconn.Open();

        try
        {
            // Get existing team standings for the same tournament(s)
            var tournamentIds = src.Select(s => s.Tournament).Distinct();
            var existingStandings = new List<TeamStanding>();

            foreach (int tournamentId in tournamentIds)
            {
                var filter = new Filter { TournamentId = tournamentId };
                var standings = await GetList(filter);
                existingStandings.AddRange(standings);
            }

            // Use SyncMaster to compare source and destination
            SyncMaster<TeamStanding> syncMaster = new(src, existingStandings);

            // Lists to store pending changes
            List<TeamStanding> addList = new();
            List<TeamStanding> updateList = new();
            List<TeamStanding> deleteList = new();

            // Configure sync event handlers
            syncMaster.Add += (s, standingToAdd) =>
            {
                addList.Add(standingToAdd);
            };

            syncMaster.Update += (s, e) =>
            {
                if (e.Source != null)
                {
                    // Copy the database ID from destination to source for update
                    if (e.Dest != null)
                        e.Source.Id = e.Dest.Id;
                    updateList.Add(e.Source);
                }
            };

            syncMaster.Remove += (s, standingToRemove) =>
            {
                deleteList.Add(standingToRemove);
            };

            // Run the synchronization analysis
            syncMaster.Run();

            // Apply database changes within a transaction
            var localTran = _dbconn.BeginTransaction();

            try
            {
                // Add new team standings
                foreach (TeamStanding standing in addList)
                {
                    InsertTeamStanding(standing, localTran);
                }

                // Update existing team standings
                foreach (TeamStanding standing in updateList)
                {
                    UpdateTeamStanding(standing, localTran);
                }

                // Delete removed team standings 
                foreach (TeamStanding standing in deleteList)
                {
                    DeleteTeamStanding(standing, localTran);
                }

                localTran.Commit();
            }
            catch (Exception)
            {
                localTran.Rollback();
                throw;
            }
        }
        finally
        {
            _dbconn.Close();
        }
    }

    /// <summary>
    /// Inserts a new team standing record using a database transaction.
    /// </summary>
    /// <param name="standing">The team standing to insert.</param>
    /// <param name="transaction">The database transaction to use.</param>
    private void InsertTeamStanding(TeamStanding standing, DbTransaction transaction)
    {
        var command = _dbconn.CreateCommandStoreProc("sp_set_team_standing",
            ["p_id", "p_team", "p_tournament", "p_wins", "p_losses", "p_draws", "p_points", "p_position"],
            [DBNull.Value, standing.TeamId, standing.Tournament, standing.Wins, standing.Losses, 
             standing.Draws, standing.Points, standing.Position],
            transaction);

        object? result = command.ExecuteScalar();
        standing.Id = command.GetIntegerFromScaler(result);
    }

    /// <summary>
    /// Updates an existing team standing record using a database transaction.
    /// </summary>
    /// <param name="standing">The team standing to update.</param>
    /// <param name="transaction">The database transaction to use.</param>
    private void UpdateTeamStanding(TeamStanding standing, DbTransaction transaction)
    {
        var command = _dbconn.CreateCommandStoreProc("sp_set_team_standing",
            ["p_id", "p_team", "p_tournament", "p_wins", "p_losses", "p_draws", "p_points", "p_position"],
            [standing.Id, standing.TeamId, standing.Tournament, standing.Wins, standing.Losses,
             standing.Draws, standing.Points, standing.Position],
            transaction);

        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Deletes a team standing record using a database transaction.
    /// </summary>
    /// <param name="standing">The team standing to delete.</param>
    /// <param name="transaction">The database transaction to use.</param>
    private void DeleteTeamStanding(TeamStanding standing, DbTransaction transaction)
    {
        var command = _dbconn.CreateCommandStoreProc("sp_delete_team_standing",
            ["p_id"],
            [standing.Id],
            transaction);

        command.ExecuteNonQuery();
    }
}