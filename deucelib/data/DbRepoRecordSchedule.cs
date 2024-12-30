using System.Data.Common;
using deuce.ext;

namespace deuce;
/// <summary>
/// Select the schedule for a tournament.
/// </summary>
public class DbRepoRecordSchedule : DbRepoBase<RecordSchedule>
{
    private readonly DbConnection _dbconn;

    /// <summary>
    /// Construct with a db connnection
    /// </summary>
    /// <param name="dbconn">The db connection</param>
    /// <param name="references">References</param>
    public DbRepoRecordSchedule(DbConnection dbconn)
    {
        _dbconn = dbconn;

    }

    public async override Task<List<RecordSchedule>> GetList(Filter filter)
    {
        List<RecordSchedule> result = new();

        using (DbCommand cmd = _dbconn.CreateCommand())
        {
            cmd.CommandText = "sp_get_tournament_schedule";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add(cmd.CreateWithValue("p_tournament", filter.TournamentId));

            var reader = await cmd.ExecuteReaderAsync();

            while(reader.Read())
            {
                result.Add(new RecordSchedule(reader.Parse<int>("match_id"),
                                              reader.Parse<int>("permutation"),
                                              reader.Parse<int>("round"),
                                              reader.Parse<int>("player_home"),
                                              reader.Parse<int>("player_away")));
            }


        }

        return result;
    }

}
