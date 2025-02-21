using System.Data.Common;
using System.Diagnostics;
using System.Formats.Asn1;
using deuce.ext;

namespace deuce;

/// <summary>
/// /// Insert and select matches from the database.
/// </summary>
public class DbRepoMatch : DbRepoBase<Match>
{

    /// <summary>
    /// Construct to the target database.
    /// </summary>
    /// <param name="conn">Database connection</param>
    public DbRepoMatch(DbConnection conn) : base(conn)
    {
    }

    /// <summary>
    /// Insert entries in the "match" table
    /// </summary>
    /// <param name="obj">Match object to store</param>
    /// <returns></returns>
    public override async Task SetAsync(Match obj)
    {
         _dbconn.Open();

        //Explicitly insert new rows if id < 1
        object primaryKeyId = obj.Id < 1 ? DBNull.Value : obj.Id;
        var cmd = _dbconn.CreateCommandStoreProc("sp_set_match", ["p_id", "p_permutation", "p_round", "p_tournament" ],
        
        [ primaryKeyId, obj.Permutation?.Id ?? 0, obj.Permutation?.Round?.Index ?? 0, obj.Permutation?.Round?.Tournament?.Id ?? 0 ],
        null);
        object? id = await cmd.ExecuteScalarAsync();
        obj.Id = (int)(ulong)(id ?? 0L);

        //Save home players
        foreach (Player player in obj.Home)
        {
            var cmd2 = _dbconn.CreateCommandStoreProc("sp_set_match_player", ["p_id","p_match", "p_player_home", "p_player_away"],
            [DBNull.Value, obj.Id,  player.Id,  DBNull.Value ], null);
            await cmd2.ExecuteNonQueryAsync();
        }

        foreach (Player player in obj.Away)
        {
            var cmd2 = _dbconn.CreateCommandStoreProc("sp_set_match_player", ["p_id","p_match", "p_player_home", "p_player_away"],
            [DBNull.Value, obj.Id,  DBNull.Value,  player.Id ], null);
            await cmd2.ExecuteNonQueryAsync();
        }

        _dbconn.Close();
    }
}