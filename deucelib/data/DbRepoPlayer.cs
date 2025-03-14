using System.Data.Common;
using System.Net;
using deuce.ext;

namespace deuce;
public class DbRepoPlayer : DbRepoBase<Player>
{
    

    public DbRepoPlayer(DbConnection dbconn) : base(dbconn)
    {
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
        await _dbconn.CreateReaderStoreProcAsync("sp_get_player", ["p_tournament" ], [ filter.TournamentId ],
        r =>
        {
            Player p = new()
            {
                Id = r.Parse<int>("id"),
                First = r.Parse<string>("first_name"),
                Last = r.Parse<string>("last_name"),
                Middle = r.Parse<string>("middle_name"),
                Ranking = r.Parse<double>("utr"),
                Tournament  = new () { Id = filter.TournamentId}
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

        var command = _dbconn.CreateCommandStoreProc("sp_set_player", ["p_id",  "p_first_name", "p_last_name",
        "p_middle_name", "p_tournament", "p_utr"], [primaryKeyId, obj.First, obj.Last , obj.Middle, obj.Tournament?.Id,  obj.Ranking ]);
        obj.Id = command.GetIntegerFromScaler(command.ExecuteScalar());

        _dbconn.Close();

    }
}