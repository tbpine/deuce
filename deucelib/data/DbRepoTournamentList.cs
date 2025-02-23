using System.Data.Common;
using deuce.ext;

namespace deuce;

public class DbRepoTournamentList : DbRepoBase<Tournament>
{
    //------------------------------------
    //| Internals                         |
    //------------------------------------
    private  Organization? _organization;

    public Organization Organization { set=>_organization = value; }

    /// <summary>
    /// Construct with a db connection to the target db.
    /// </summary>
    /// <param name="dbconn">Database connection</param>
    public DbRepoTournamentList(DbConnection dbconn,Organization organization) : base(dbconn)
    {
        _organization = organization;
    }

    /// <summary>
    /// Construct with a db connection to the target db.
    /// </summary>
    public DbRepoTournamentList(DbConnection dbconn) : base(dbconn)
    {
        _organization = null;
    }

    public override async Task<List<Tournament>> GetList(Filter filter)
    {
        _dbconn.Open();

        List<Tournament> list = new();
        await _dbconn.CreateReaderStoreProcAsync("sp_get_tournament_list", ["p_organization"], [filter.ClubId],
        reader=>{
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

        });
        
        _dbconn.Close();
        
        return list;
    }

   
}