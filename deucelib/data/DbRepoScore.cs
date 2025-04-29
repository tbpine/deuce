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
    /// The <see cref="Score"/> object containing the score details to be saved.
    /// If the <c>Id</c> property of the object is less than 1, a new record will be inserted.
    /// Otherwise, the existing record with the specified <c>Id</c> will be updated.
    /// </param>
    /// <remarks>
    /// This method uses a stored procedure named <c>sp_set_score</c> to perform the database operation.
    /// The following parameters are passed to the stored procedure:
    /// <list type="bullet">
    /// <item><description><c>p_id</c>: The primary key ID of the score record.</description></item>
    /// <item><description><c>p_tournament</c>: The tournament associated with the score.</description></item>
    /// <item><description><c>p_permutation</c>: The permutation associated with the score.</description></item>
    /// <item><description><c>p_match</c>: The match associated with the score.</description></item>
    /// <item><description><c>p_home</c>: The home score.</description></item>
    /// <item><description><c>p_away</c>: The away score.</description></item>
    /// <item><description><c>p_notes</c>: Additional notes about the score.</description></item>
    /// </list>
    /// The method opens a database connection, executes the stored procedure, and retrieves the generated
    /// primary key ID (if a new record is inserted) using <c>ExecuteScalar</c>.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the database connection cannot be opened or if the stored procedure execution fails.
    /// </exception>
    public override void Set(Score obj)
    {
        _dbconn.Open();
        //Explicitly insert new rows if id < 1
        object primaryKeyId = obj.Id < 1 ? DBNull.Value : obj.Id;

        var command = _dbconn.CreateCommandStoreProc("sp_set_score", ["p_id",  "p_tournament", "p_permutation",
        "p_match", "p_home", "p_away", "p_notes"], [primaryKeyId, obj.Tournament, obj.Permutation , obj.Match, obj.Home,  obj.Away ,
        obj.Notes]);

        obj.Id = command.GetIntegerFromScaler(command.ExecuteScalar());

        _dbconn.Close();

    }
}