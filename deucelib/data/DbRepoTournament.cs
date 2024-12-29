using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
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


    /// <summary>
    /// Construct with dependencies
    /// </summary>
    /// <param name="dbconn">Db Connection</param>
    public DbRepoTournament(DbConnection dbconn,params object[] references)
    {
        _dbconn = dbconn;
        _types = references[0] as List<TournamentType>;
        _intervals = references[0] as List<Interval>;
    }

    /// <summary>
    /// Construct with a db connection to the target db.
    /// </summary>
    /// <param name="dbconn">Database connection</param>
    public DbRepoTournament(DbConnection dbconn)
    {
        _dbconn = dbconn;
        _types = new List<TournamentType>();
        _intervals = new List<Interval>();
    }

    public override async Task<List<Tournament>> GetList()
    {
        //Open Connection
        await _dbconn.OpenAsync();

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

            Interval? inter = _intervals?.Find(e => e.Id == id);
            TournamentType? tt = _types?.Find(e => e.Id == id);

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
                UseRanking = useRanking
            });
        }
        return list;
    }
}