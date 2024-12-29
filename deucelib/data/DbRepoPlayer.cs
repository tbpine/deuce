using System.Data.Common;
using deuce.ext;

namespace deuce;
public class DbRepoPlayer : DbRepoBase<Player>
{
    private readonly DbConnection _dbconn;
    private readonly Club? _club;

    public DbRepoPlayer(DbConnection dbconn, params object[] references)
    {
        _dbconn = dbconn;
        _club = references[0] as Club;
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

            cmd.Parameters.Add(cmd.CreateWithValue("p_club_id", filter.ClubId));
            var reader = await cmd.ExecuteReaderAsync();

            while (reader.Read())
            {
                Player p = new() {
                    Id = reader.Parse<int>("id"),
                    Club = _club,
                    First = reader.Parse<string>("first_name"),
                    Last = reader.Parse<string>("last_name"),
                    Ranking = reader.Parse<double>("utr")
                };

                players.Add(p);
            }
        }

        return players;
        
    }
}