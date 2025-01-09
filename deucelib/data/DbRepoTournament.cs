using System.Data;
using System.Data.Common;
using System.Diagnostics;
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
    private readonly List<TournamentType>? _types;
    private readonly List<Interval>? _intervals;
    private readonly List<Sport>? _sports;
    private readonly Organization? _organization;

    /// <summary>
    /// Construct with dependencies
    /// </summary>
    /// <param name="dbconn">Db Connection</param>
    public DbRepoTournament(DbConnection dbconn,List<TournamentType> types, List<Interval> intervals,
    List<Sport> sports, Organization organization)
    {
        _dbconn = dbconn;
        _types = types;
        _intervals = intervals;
        _sports = sports;
        _organization = organization;

    }

    /// <summary>
    /// Construct with a db connection to the target db.
    /// </summary>
    /// <param name="dbconn">Database connection</param>
    public DbRepoTournament(DbConnection dbconn,Organization organization)
    {
        _dbconn = dbconn;
        _organization = organization;
    }

    public override async Task<List<Tournament>> GetList()
    {
        DbCommand cmd = _dbconn.CreateCommand();
        cmd.CommandText = "sp_get_tournament";
        cmd.CommandType = CommandType.StoredProcedure;

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

            Interval? inter = _intervals?.Find(e => e.Id == interval);
            TournamentType? tt = _types?.Find(e => e.Id == type);
            Sport? sport = _sports?.Find(e => e.Id == sportId );

            list.Add(new Tournament
            {
                Id = id,
                Label = label,
                Start = start,
                End = end,
                Interval = inter,
                Steps = steps,
                Type = tt,
                Max = max,
                Fee = fee,
                Prize = prize,
                UseRanking = useRanking,
                Sport = sport,
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
        cmd.Parameters.Add(cmd.CreateWithValue("p_interval", obj.Interval?.Id??4)); //Weekly default
        cmd.Parameters.Add(cmd.CreateWithValue("p_steps", obj.Steps));
        cmd.Parameters.Add(cmd.CreateWithValue("p_type", obj.Type?.Id??1));
        cmd.Parameters.Add(cmd.CreateWithValue("p_max", obj.Max));
        cmd.Parameters.Add(cmd.CreateWithValue("p_fee", obj.Fee));
        cmd.Parameters.Add(cmd.CreateWithValue("p_prize", obj.Prize));
        cmd.Parameters.Add(cmd.CreateWithValue("p_seedings", obj.UseRanking ? 1:0));
        cmd.Parameters.Add(cmd.CreateWithValue("p_sport", obj.Sport?.Id?? 1)); //default to tennis
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