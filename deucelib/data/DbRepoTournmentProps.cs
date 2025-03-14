using System.Data.Common;
using deuce.ext;

namespace deuce;
public class DbRepoTournamentProps : DbRepoBase<Tournament>
{

    /// <summary>
    /// Construct with dependency
    /// </summary>
    /// <param name="dbconn"></param>
    public DbRepoTournamentProps(DbConnection dbconn) : base(dbconn)
    {
    }

    public override async Task SetAsync(Tournament obj)
    {
        _dbconn.Open();

        //Insert into the team table
        var localtran = _dbconn.BeginTransaction();
        var cmd = _dbconn.CreateCommandStoreProc("sp_set_tournament_schedule", ["p_tournament", "p_interval", "p_start"],
        [obj.Id, obj.Interval, obj.Start ], localtran);

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