using System.Data.Common;
using deuce.ext;

namespace deuce;


public class DbRepoScore : DbRepoBase<Score>
{
    public DbRepoScore(DbConnection dbconn) : base(dbconn)
    {
    }

    /// <summary>
    /// Inserts or updates a score record in the database.
    /// </summary>
    /// <param name="obj">
    /// The <see cref="Score"/> object containing the score details to be stored.
    /// If the <c>Id</c> property of the object is less than 1, a new record will be inserted.
    /// Otherwise, the existing record with the specified <c>Id</c> will be updated.
    /// </param>
    /// <remarks>
    /// This method uses a stored procedure named "sp_set_score" to perform the database operation.
    /// The database connection is opened before executing the command and closed afterward.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the database connection cannot be opened or the stored procedure execution fails.
    /// </exception>
    public override void Set(Score obj)
    {
        _dbconn.Open();
        //Explicitly insert new rows if id < 1
        object primaryKeyId = obj.Id < 1 ? DBNull.Value : obj.Id;
        var localTran = _dbconn.BeginTransaction();
        try
        {
            var command = _dbconn.CreateCommandStoreProc("sp_set_score", ["p_id",  "p_tournament","p_round", "p_permutation",
        "p_match", "p_home", "p_away", "p_set", "p_notes"], [primaryKeyId, obj.Tournament, obj.Round, obj.Permutation , obj.Match, obj.Home,  obj.Away ,
         obj.Set, obj.Notes], localTran);
            obj.Id = command.GetIntegerFromScaler(command.ExecuteScalar());
            localTran.Commit();


        }
        catch (DbException ex)
        {
            localTran.Rollback();
            throw new InvalidOperationException("Error executing stored procedure.", ex);
        }


        _dbconn.Close();

    }

    //Override async Task<List<Score>> GetList(Filter filter)
    // to return a list of Score objects from the database based on the provided filter criteria.
    // The method uses a stored procedure named "sp_get_score" to retrieve the data.
    // The filter parameter is used to specify the criteria for filtering the results.
    // The method opens a database connection, executes the stored procedure, and reads the results into a list of Score objects.
    // Finally, it closes the database connection and returns the list of Score objects.

    public override async Task<List<Score>> GetList(Filter filter)
    {
        var scores = new List<Score>();
        _dbconn.Open();
        var command = _dbconn.CreateCommandStoreProc("sp_get_score",
            new[] { "p_tournament" },
            new object[] { filter.TournamentId });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var score = new Score
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Tournament = reader.GetInt32(reader.GetOrdinal("Tournament")),
                Round = reader.GetInt32(reader.GetOrdinal("Round")),
                Permutation = reader.GetInt32(reader.GetOrdinal("Permutation")),
                Match = reader.GetInt32(reader.GetOrdinal("Match")),
                Home = reader.GetInt32(reader.GetOrdinal("Home")),
                Away = reader.GetInt32(reader.GetOrdinal("Away")),
                Set = reader.GetInt32(reader.GetOrdinal("Set")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Notes"))
            };
            scores.Add(score);
        }
        _dbconn.Close();
        return scores;

    }

    //clear scores by calling the stored procedure "sp_clear_score"
    public void ClearScores(int tournamentId)
    {
        _dbconn.Open();
        var localTran = _dbconn.BeginTransaction();
        var command = _dbconn.CreateCommandStoreProc("sp_clear_score", ["p_tournament"]
        , [tournamentId], localTran);
        try
        {
            command.ExecuteNonQuery();
            localTran.Commit();
        }
        catch (DbException ex)
        {
            localTran.Rollback();
            throw new InvalidOperationException("Error executing stored procedure.", ex);   
        }

        _dbconn.Close();
    }

}