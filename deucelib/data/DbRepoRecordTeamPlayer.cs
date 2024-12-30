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
            var reader = await cmd.ExecuteReaderAsync();

            while (reader.Read())
            {
                RecordTeamPlayer recordTeamPlayer = new(
                    reader.Parse<int>("id"),
                    reader.Parse<int>("club"),
                    reader.Parse<int>("tournament"),
                    reader.Parse<string>("team"),
                    reader.Parse<int>("team_id"),
                    reader.Parse<int>("player_id"),
                    reader.Parse<string>("first_name"),
                    reader.Parse<string>("last_name"),
                    reader.Parse<double>("utr")

                );

                list.Add(recordTeamPlayer);
            }

            reader.Close();
        }

        return list;
        
    }
}