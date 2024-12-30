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
    private DbConnection _dbconn;

    /// <summary>
    /// Construct to the target database.
    /// </summary>
    /// <param name="conn">Database connection</param>
    public DbRepoMatch(DbConnection conn)
    {
        _dbconn = conn;
    }

    /// <summary>
    /// Insert entries in the "match" table
    /// </summary>
    /// <param name="obj">Match object to store</param>
    /// <returns></returns>
    public override async Task Set(Match obj)
    {


        using (DbCommand cmd = _dbconn.CreateCommand())
        {
            cmd.CommandText = "sp_set_match";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.Add(cmd.CreateWithValue("p_id", obj.Id <= 0 ? DBNull.Value : obj.Id));
            cmd.Parameters.Add(cmd.CreateWithValue("p_permutation", obj.Permutation?.Id ?? 0));
            cmd.Parameters.Add(cmd.CreateWithValue("p_round", obj.Permutation?.Round?.Index ?? 0));
            cmd.Parameters.Add(cmd.CreateWithValue("p_tournament", obj.Permutation?.Round?.Tournament?.Id ?? 0));
            object? id = await cmd.ExecuteScalarAsync();
            obj.Id = (int)(ulong)(id ?? 0L);

        }

        //Save players
        using (DbCommand cmd = _dbconn.CreateCommand())
        {
            cmd.CommandText = "sp_set_match_player";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.Add(cmd.CreateWithValue("p_id", DBNull.Value));
            cmd.Parameters.Add(cmd.CreateWithValue("p_match", 0));
            cmd.Parameters.Add(cmd.CreateWithValue("p_player_home", 0));
            cmd.Parameters.Add(cmd.CreateWithValue("p_player_away", 0));

            //Save home players
            foreach (Player player in obj.Home)
            {
                cmd.Parameters["p_match"].Value = obj.Id;
                cmd.Parameters["p_player_home"].Value = player.Id;
                cmd.Parameters["p_player_away"].Value = DBNull.Value;

                await cmd.ExecuteNonQueryAsync();
            }

            //Save away players
            foreach (Player player in obj.Away)
            {
                cmd.Parameters["p_match"].Value = obj.Id;
                cmd.Parameters["p_player_home"].Value = DBNull.Value;
                cmd.Parameters["p_player_away"].Value = player.Id;

                await cmd.ExecuteNonQueryAsync();
            }


        }

    }
}