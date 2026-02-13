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
        //get tournament id
        int tourId = obj.Permutation?.Round?.Tournament?.Id ?? 0;

        var cmd = _dbconn.CreateCommandStoreProc("sp_set_match", ["p_id", "p_permutation", "p_round", "p_tournament" ,"p_players_per_side"],
        
        [ primaryKeyId, obj.Permutation?.Id ?? 0, obj.Permutation?.Round?.Index ?? 0, tourId, obj.PlayersPerSide],
        null);
        object? id = await cmd.ExecuteScalarAsync();
        obj.Id = id == null || id == DBNull.Value ? 0 : Convert.ToInt32(id);

        // Synchronize match_player records if match already exists
        if (obj.Id > 0)
        {
            // Get existing match_player records
            var getExistingCmd = _dbconn.CreateCommand();
            getExistingCmd.CommandText = "SELECT `id`, `player_home`, `player_away` FROM `match_player` WHERE `match` = @matchId";
            var matchIdParam = getExistingCmd.CreateParameter();
            matchIdParam.ParameterName = "@matchId";
            matchIdParam.Value = obj.Id;
            getExistingCmd.Parameters.Add(matchIdParam);
            
            var existingRecords = new List<(int Id, int? PlayerHome, int? PlayerAway)>();
            using (var reader = await getExistingCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var playerHome = reader.IsDBNull("player_home") ? (int?)null : reader.GetInt32("player_home");
                    var playerAway = reader.IsDBNull("player_away") ? (int?)null : reader.GetInt32("player_away");
                    existingRecords.Add((reader.GetInt32("id"), playerHome, playerAway));
                }
            }

            // Build current schedule players
            var currentPlayers = new List<(int? PlayerHome, int? PlayerAway)>();
            
            // Add home players (player_home set, player_away is null)
            foreach (Player player in obj.Home)
            {
                int? playerId = player.Id <= 0 ? null : player.Id;
                currentPlayers.Add((playerId, null));
            }
            
            // Add away players (player_away set, player_home is null)
            foreach (Player player in obj.Away)
            {
                int? playerId = player.Id <= 0 ? null : player.Id;
                currentPlayers.Add((null, playerId));
            }

            // Find records to delete (exist in DB but not in current schedule)
            var recordsToDelete = existingRecords.Where(existing => 
                !currentPlayers.Any(current => 
                    current.PlayerHome == existing.PlayerHome && 
                    current.PlayerAway == existing.PlayerAway)).ToList();

            // Find records to insert (exist in current schedule but not in DB)
            var recordsToInsert = currentPlayers.Where(current => 
                !existingRecords.Any(existing => 
                    existing.PlayerHome == current.PlayerHome && 
                    existing.PlayerAway == current.PlayerAway)).ToList();

            // Delete obsolete records
            foreach (var record in recordsToDelete)
            {
                var deleteCmd = _dbconn.CreateCommand();
                deleteCmd.CommandText = "DELETE FROM `match_player` WHERE `id` = @recordId";
                var recordIdParam = deleteCmd.CreateParameter();
                recordIdParam.ParameterName = "@recordId";
                recordIdParam.Value = record.Id;
                deleteCmd.Parameters.Add(recordIdParam);
                await deleteCmd.ExecuteNonQueryAsync();
            }

            // Insert new records
            foreach (var record in recordsToInsert)
            {
                object homePlayerId = record.PlayerHome?.ToString() ?? (object)DBNull.Value;
                object awayPlayerId = record.PlayerAway?.ToString() ?? (object)DBNull.Value;
                
                var cmd2 = _dbconn.CreateCommandStoreProc("sp_set_match_player", ["p_id","p_match", "p_player_home", "p_player_away", "p_tournament"],
                [DBNull.Value, obj.Id, homePlayerId, awayPlayerId, tourId], null);
                await cmd2.ExecuteNonQueryAsync();
            }
        }
        else
        {
            // New match - insert all players
            foreach (Player player in obj.Home)
            {
                //Null in the player id means the bye player
                object normalizedId = player.Id <= 0 ? DBNull.Value : player.Id;
                var cmd2 = _dbconn.CreateCommandStoreProc("sp_set_match_player", ["p_id","p_match", "p_player_home", "p_player_away", "p_tournament"],
                [DBNull.Value, obj.Id, normalizedId, DBNull.Value, tourId], null);
                await cmd2.ExecuteNonQueryAsync();
            }

            foreach (Player player in obj.Away)
            {
                //Null in the player id means the bye player
                object normalizedId = player.Id <= 0 ? DBNull.Value : player.Id;
                var cmd2 = _dbconn.CreateCommandStoreProc("sp_set_match_player", ["p_id","p_match", "p_player_home", "p_player_away", "p_tournament"],
                [DBNull.Value, obj.Id, DBNull.Value, normalizedId, tourId], null);
                await cmd2.ExecuteNonQueryAsync();
            }
        }

        _dbconn.Close();
    }
}