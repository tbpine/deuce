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
            localTran.Commit();
            obj.Id = command.GetIntegerFromScaler(command.ExecuteScalar());


        }
        catch (DbException ex)
        {
            localTran.Rollback();
            throw new InvalidOperationException("Error executing stored procedure.", ex);
        }


        _dbconn.Close();

    }
}