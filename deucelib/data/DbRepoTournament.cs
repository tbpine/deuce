using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Transactions;
using deuce.ext;

namespace deuce;

public class DbRepoTournament : DbRepoBase<Tournament>
{
    //------------------------------------
    //| Internals                         |
    //------------------------------------
    private readonly DbConnection _dbconn;
    private readonly Organization? _organization;

    /// <summary>
    /// Construct with a db connection to the target db.
    /// </summary>
    /// <param name="dbconn">Database connection</param>
    public DbRepoTournament(DbConnection dbconn,Organization organization)
    {
        _dbconn = dbconn;
        _organization = organization;
    }

    public override async Task<List<Tournament>> GetList(Filter filter)
    {
        DbCommand cmd = _dbconn.CreateCommand();
        cmd.CommandText = "sp_get_tournament";
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add(cmd.CreateWithValue("p_id", filter.TournamentId));
        
        DbDataReader reader = await cmd.ExecuteReaderAsync();
        List<Tournament> list = new();

        while (reader.Read())
        {
            int id = reader.Parse<int>("id");
            string label = reader.Parse<string>("label");
            DateTime start = reader.Parse<DateTime>("start");
            DateTime end = reader.Parse<DateTime>("end");

            int interval = reader.Parse<int>("interval");
            int steps = reader.Parse<int>("steps");
            int type = reader.Parse<int>("type");
            int max = reader.Parse<int>("max");

            float fee = reader.Parse<float>("fee");
            float prize = reader.Parse<float>("prize");
            bool useRanking = reader.Parse<int>("seedings") == 1;
            int sportId = reader.Parse<int>("sport");

            list.Add(new Tournament
            {
                Id = id,
                Label = label,
                Start = start,
                End = end,
                Interval = interval,
                Steps = steps,
                Type = type,
                Max = max,
                Fee = fee,
                Prize = prize,
                UseRanking = useRanking,
                Sport = sportId,
                Organization = _organization
            
            });
        }
        return list;
    }

    public override async Task Set(Tournament obj) 
    {
        DbCommand cmd = _dbconn.CreateCommand();
        cmd.CommandText = "sp_set_tournament";
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.Add(cmd.CreateWithValue("p_id", obj.Id <1 ? DBNull.Value : obj.Id));
        cmd.Parameters.Add(cmd.CreateWithValue("p_label", obj.Label??""));
        cmd.Parameters.Add(cmd.CreateWithValue("p_start", obj.Start));
        cmd.Parameters.Add(cmd.CreateWithValue("p_end",obj.End));
        cmd.Parameters.Add(cmd.CreateWithValue("p_interval", obj.Interval)); //Weekly default
        cmd.Parameters.Add(cmd.CreateWithValue("p_steps", obj.Steps));
        cmd.Parameters.Add(cmd.CreateWithValue("p_type", obj.Type));
        cmd.Parameters.Add(cmd.CreateWithValue("p_max", obj.Max));
        cmd.Parameters.Add(cmd.CreateWithValue("p_fee", obj.Fee));
        cmd.Parameters.Add(cmd.CreateWithValue("p_prize", obj.Prize));
        cmd.Parameters.Add(cmd.CreateWithValue("p_seedings", obj.UseRanking ? 1:0));
        cmd.Parameters.Add(cmd.CreateWithValue("p_sport", obj.Sport)); //default to tennis
        cmd.Parameters.Add(cmd.CreateWithValue("p_organization", obj.Organization?.Id??1)); //default to tennis

        var localTran = _dbconn.BeginTransaction();
        object? objret = null;
        try{
            objret = await cmd.ExecuteScalarAsync();
            localTran.Commit();

        }
        catch(Exception ex)
        {
            localTran.Rollback();
            Debug.WriteLine(ex.Message);
        }
        
        ulong newId = objret is null ? 0L : (ulong)objret;
        obj.Id = (int)newId;

    }

}