using System.Data.Common;
using deuce.ext;

namespace deuce;
public class DbRepoTournamentProps : DbRepoBase<Tournament>
{
    private readonly DbConnection _dbconn;

    /// <summary>
    /// Construct with dependency
    /// </summary>
    /// <param name="dbconn"></param>
    public DbRepoTournamentProps(DbConnection dbconn)
    {
        _dbconn = dbconn;
    }

    public override async Task SetAsync(Tournament obj)
    {

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


    }

}