using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Transactions;
using deuce.ext;

namespace deuce;

public class DbRepoTournamentList : DbRepoBase<Tournament>
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
    public DbRepoTournamentList(DbConnection dbconn,Organization organization)
    {
        _dbconn = dbconn;
        _organization = organization;
    }

    public override async Task<List<Tournament>> GetList(Filter filter)
    {
        DbCommand cmd = _dbconn.CreateCommand();
        cmd.CommandText = "sp_get_tournament_list";
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add(cmd.CreateWithValue("p_organization", filter.ClubId));
        
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
        await Task.CompletedTask;
    }

}