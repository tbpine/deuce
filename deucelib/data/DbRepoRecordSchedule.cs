using System.Data.Common;
using deuce.ext;
using Org.BouncyCastle.Cms;

namespace deuce;
/// <summary>
/// Select the schedule for a tournament.
/// </summary>
public class DbRepoRecordSchedule : DbRepoBase<RecordSchedule>
{
    
    /// <summary>
    /// Construct with a db connnection
    /// </summary>
    /// <param name="dbconn">The db connection</param>
    /// <param name="references">References</param>
    public DbRepoRecordSchedule(DbConnection dbconn) : base(dbconn)
    {
    }

    public async override Task<List<RecordSchedule>> GetList(Filter filter)
    {
        _dbconn.Open();

        List<RecordSchedule> result = new();
        await _dbconn.CreateReaderStoreProcAsync("sp_get_tournament_schedule", ["p_tournament" ], [ filter.TournamentId ],
        r =>
        {
            result.Add(new RecordSchedule(r.Parse<int>("match_id"),
                                        r.Parse<int>("permutation"),
                                        r.Parse<int>("round"),
                                        r.Parse<int>("player_home"),
                                        r.Parse<int>("player_away"),
                                        r.Parse<int>("team_home"),
                                        r.Parse<int>("team_away"),
                                        r.Parse<int>("players_per_side")
                                                          ));
        });

        _dbconn.Close();
        
        return result;
    }

}
