using System.Data.Common;
using deuce.ext;

namespace deuce;

/// <summary>
/// Update  tournament status.
/// </summary>
public class DbRepoTournamentStatus : DbRepoBase<Tournament>
{

    /// <summary>
    /// Construct with dependency
    /// </summary>
    /// <param name="dbconn"></param>
    public DbRepoTournamentStatus(DbConnection dbconn) : base(dbconn)
    {
    }

    public override async Task SetAsync(Tournament obj)
    {
        _dbconn.Open();

        //Insert into the team table
        var localtran = _dbconn.BeginTransaction();
        var cmd = _dbconn.CreateCommandStoreProc("sp_set_tournament_status", ["p_tournament", "p_status"],
        [obj.Id, obj.Status ], localtran);

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