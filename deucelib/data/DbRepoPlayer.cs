using System.Data.Common;
using deuce.ext;

namespace deuce;
public class DbRepoPlayer : DbRepoBase<Player>
{
    private readonly DbConnection _dbconn;
    private readonly Organization? _club;

    public DbRepoPlayer(DbConnection dbconn, params object[] references)
    {
        _dbconn = dbconn;
        _club = references[0] as Organization;
    }

    /// <summary>
    /// get a list of players from a club
    /// </summary>
    /// <param name="filter">Filter object</param>
    /// <returns></returns>
    public override async Task<List<Player>> GetList(Filter filter)
    {
        List<Player> players = new();
        using (DbCommand cmd = _dbconn.CreateCommand())
        {
            cmd.CommandText = "sp_get_player";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.Add(cmd.CreateWithValue("p_organization", filter.ClubId));
            var reader = new SafeDataReader(await cmd.ExecuteReaderAsync());

            while (reader.Target.Read())
            {
                Player p = new() {
                    Id = reader.Target.Parse<int>("id"),
                    Club = _club,
                    First = reader.Target.Parse<string>("first_name"),
                    Last = reader.Target.Parse<string>("last_name"),
                    Ranking = reader.Target.Parse<double>("utr")
                };

                players.Add(p);
            }

            reader.Target.Close();
        }

        return players;
        
    }
}