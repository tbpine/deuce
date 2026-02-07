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
}