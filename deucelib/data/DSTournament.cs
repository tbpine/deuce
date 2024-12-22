using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using deuce.ext;

namespace deuce;

class DSTournament
{
    //------------------------------------
    //| Internals                         |
    //------------------------------------
    private readonly DbConnection _dbconn;
    private readonly List<TournamentType> _types;
    private readonly List<Interval> _intervals;


    /// <summary>
    /// Construct with dependencies
    /// </summary>
    /// <param name="dbconn">Db Connection</param>
    public DSTournament(DbConnection dbconn, List<TournamentType> ts, List<Interval> intervals)
    {
        _dbconn = dbconn;
        _types = ts;
        _intervals = intervals;
    }

    public async Task<List<Tournament>> GetList()
    {
        //Open Connection
        await _dbconn.OpenAsync();
        
        DbCommand cmd = _dbconn.CreateCommand();
        cmd.CommandText = "sp_get_tournament";
        cmd.CommandType = CommandType.StoredProcedure;

        DbDataReader reader = await cmd.ExecuteReaderAsync();
        List<Tournament> list=new();

        while(reader.Read())
        {
            int id = reader.Parse<int>("id");
            string label = reader.Parse<string>("label");
            DateTime start =  reader.Parse<DateTime>("start");
            DateTime end =  reader.Parse<DateTime>("end");

            int interval = reader.Parse<int>("interval");
            int steps = reader.Parse<int>("steps");
            int type = reader.Parse<int>("type");
            int max = reader.Parse<int>("max");
            
            float fee = reader.Parse<float>("fee");
            float prize = reader.Parse<float>("prize");
            bool useRanking  = reader.Parse<int>("seedings") == 1;

            Interval? inter = _intervals.Find(e=>e.Id == id);
            TournamentType? tt = _types.Find(e=>e.Id == id);

            list.Add(new Tournament{
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