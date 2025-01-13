using System.Data.Common;
using deuce.ext;

namespace deuce;
public class DbRepoRecordTeamPlayer : DbRepoBase<RecordTeamPlayer>
{
    private readonly DbConnection _dbconn;
    public DbRepoRecordTeamPlayer(DbConnection dbconn)
    {
        _dbconn = dbconn;
    }

    /// <summary>
    /// get a list of players from a club
    /// </summary>
    /// <param name="filter">Filter object</param>
    /// <returns></returns>
    public override async Task<List<RecordTeamPlayer>> GetList(Filter filter)
    {
        List<RecordTeamPlayer> list = new();
        using (DbCommand cmd = _dbconn.CreateCommand())
        {
            cmd.CommandText = "sp_get_team_player";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.Add(cmd.CreateWithValue("p_tournament", filter.TournamentId));
            var reader = new SafeDataReader(await cmd.ExecuteReaderAsync());

            while (reader.Target.Read())
            {
                RecordTeamPlayer recordTeamPlayer = new(
                    reader.Target.Parse<int>("id"),
                    reader.Target.Parse<int>("club"),
                    reader.Target.Parse<int>("tournament"),
                    reader.Target.Parse<string>("team"),
                    reader.Target.Parse<int>("team_id"),
                    reader.Target.Parse<int>("player_id"),
                    reader.Target.Parse<string>("first_name"),
                    reader.Target.Parse<string>("last_name"),
                    reader.Target.Parse<double>("utr")

                );

                list.Add(recordTeamPlayer);
            }

            reader.Target.Close();
        }

        return list;
        
    }
}