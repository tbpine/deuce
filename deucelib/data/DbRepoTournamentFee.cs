using System.Data.Common;
using deuce.ext;

namespace deuce;

/// <summary>
/// Update the fee entry fee for a tournament.
/// </summary>
public class DbRepoTournamentFee : DbRepoBase<Tournament>
{
    /// <summary>
    /// Construct with dependency
    /// </summary>
    /// <param name="dbconn"></param>
    public DbRepoTournamentFee(DbConnection dbconn) : base(dbconn)
    {
    }

    public override async Task SetAsync(Tournament obj)
    {
        _dbconn.Open();

        //Insert into the team table
        var localtran = _dbconn.BeginTransaction();
        var cmd = _dbconn.CreateCommandStoreProc("sp_set_tournament_fee", ["p_tournament", "p_fee"],
        [obj.Id, obj.Fee ], localtran);

        try
        {
            await cmd.ExecuteNonQueryAsync();
            localtran.Commit();
        }
        catch (Exception)
        {
            localtran.Rollback();

        }
        finally
        {
            _dbconn.Close();
        }

    }

}