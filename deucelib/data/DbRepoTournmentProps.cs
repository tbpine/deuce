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

    public override async Task Set(Tournament obj)
    {

        //Insert into the team table
        var cmd = _dbconn.CreateCommand();
        var localtran = _dbconn.BeginTransaction();
        cmd.CommandText = "sp_set_tournament_schedule";
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.Transaction = localtran;
        cmd.Parameters.Add(cmd.CreateWithValue("p_tournament", obj.Id));
        cmd.Parameters.Add(cmd.CreateWithValue("p_interval", obj.Interval));
        cmd.Parameters.Add(cmd.CreateWithValue("p_start", obj.Start));

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