using System.Data.Common;
using deuce.ext;

namespace deuce;
public class DbRepoPlayer : DbRepoBase<Player>
{
    
    private readonly Organization? _organization;

    public DbRepoPlayer(DbConnection dbconn, params object[] references) : base(dbconn)
    {
        _organization = references[0] as Organization;
    }

    public DbRepoPlayer(DbConnection dbconn, Organization organization) : base(dbconn)
    {
        _organization = organization;
    }

    public DbRepoPlayer(DbConnection dbconn) : base(dbconn)
    {
        _organization = null;
    }
    
    /// <summary>
    /// get a list of players from a club
    /// </summary>
    /// <param name="filter">Filter object</param>
    /// <returns></returns>
    public override async Task<List<Player>> GetList(Filter filter)
    {
        _dbconn.Open();

        List<Player> players = new();
        await _dbconn.CreateReaderStoreProcAsync("sp_get_player", ["p_organization" ], [ filter.ClubId ],
        r =>
        {
            Player p = new()
            {
                Id = r.Parse<int>("id"),
                Club = _organization,
                First = r.Parse<string>("first_name"),
                Last = r.Parse<string>("last_name"),
                Ranking = r.Parse<double>("utr")
            };

            players.Add(p);

        });

        _dbconn.Close();

        return players;

    }

    public override void Set(Player obj)
    {
        _dbconn.Open();
         //Explicitly insert new rows if id < 1
        object primaryKeyId = obj.Id < 1 ? DBNull.Value : obj.Id;

        var command = _dbconn.CreateCommandStoreProc("sp_set_player", ["p_id", "p_organization", "p_first_name", "p_last_name",
        "p_utr"], [primaryKeyId, _organization?.Id ?? 1, obj.First ?? "", obj.Last ?? "", obj.Ranking ]);
        obj.Id = command.GetIntegerFromScaler(command.ExecuteScalar());

        _dbconn.Close();

    }
}