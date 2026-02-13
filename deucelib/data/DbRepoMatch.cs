using System.Data.Common;
using System.Diagnostics;
using System.Formats.Asn1;
using deuce.ext;

namespace deuce;

/// <summary>
/// Internal class to represent match_player table data
/// </summary>
public class MatchPlayerRecord
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public int? HomePlayerId { get; set; }
    public int? AwayPlayerId { get; set; }
}

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
            Match match = obj as Match;
            if (match == null)
            {
                throw new InvalidOperationException("Object is not of type Match");
            }

            // Get existing match_player records using GetMatchPlayers
            var existingRecords = await GetMatchPlayers(obj.Id);
            List<Player> addHomePlayers = new List<Player>();   
            List<Player> addAwayPlayers = new List<Player>();   
            List<Player> delHomePlayers = new List<Player>();   
            List<Player> delAwayPlayers = new List<Player>(); 

            foreach(Player homePlayers in match.Home)
            {
                if (existingRecords.Find(r=>r.HomePlayerId == homePlayers.Id) == null)
                {
                    addHomePlayers.Add(homePlayers);
                }
            } 

            foreach(Player awayPlayers in ((Match)obj).Away)
            {
                if (existingRecords.Find(r=>r.AwayPlayerId == awayPlayers.Id) == null)
                {
                    addAwayPlayers.Add(awayPlayers);
                }
            } 
            
            //Go through existing records . If there's no home player in the match,
            //then add it to the delete home player list.
            existingRecords.ForEach(r=> {
                if (r.HomePlayerId != null && !match.Home.Any(p=>p.Id == r.HomePlayerId))
                {
                    delHomePlayers.Add(new Player{Id = r.HomePlayerId.Value});
                }
                if (r.AwayPlayerId != null && !match.Away.Any(p=>p.Id == r.AwayPlayerId))
                {
                    delAwayPlayers.Add(new Player{Id = r.AwayPlayerId.Value});
                }

            });

        
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

    /// <summary>
    /// Get list of match player records for a specific match using the sp_get_match_player stored procedure
    /// </summary>
    /// <param name="matchId">The ID of the match to get players for</param>
    /// <returns>List of MatchPlayerRecord objects from the match_player table</returns>
    public async Task<List<MatchPlayerRecord>> GetMatchPlayers(int matchId)
    {
        var matchPlayerRecords = new List<MatchPlayerRecord>();
        _dbconn.Open();

        try
        {
            var command = _dbconn.CreateCommandStoreProc("sp_get_match_player",
                new[] { "p_match_id" },
                new object[] { matchId });

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var record = new MatchPlayerRecord
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    MatchId = reader.IsDBNull(reader.GetOrdinal("match")) ? 0 : reader.GetInt32(reader.GetOrdinal("match")),
                    HomePlayerId = reader.IsDBNull(reader.GetOrdinal("player_home")) ? null : reader.GetInt32(reader.GetOrdinal("player_home")),
                    AwayPlayerId = reader.IsDBNull(reader.GetOrdinal("player_away")) ? null : reader.GetInt32(reader.GetOrdinal("player_away"))
                };

                matchPlayerRecords.Add(record);
            }
        }
        finally
        {
            _dbconn.Close();
        }

        return matchPlayerRecords;
    }

    /// <summary>
    /// Add a home player to a match using the sp_set_match_player stored procedure
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="playerId">The ID of the home player to add</param>
    /// <param name="tournamentId">The ID of the tournament</param>
    /// <returns>The ID of the created match_player record</returns>
    public async Task<int> AddHomePlayer(int matchId, int playerId, int tournamentId)
    {
        _dbconn.Open();

        try
        {
            var command = _dbconn.CreateCommandStoreProc("sp_set_match_player",
                new[] { "p_id", "p_match", "p_player_home", "p_player_away", "p_tournament" },
                new object[] { DBNull.Value, matchId, playerId, DBNull.Value, tournamentId });

            var result = await command.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }
        finally
        {
            _dbconn.Close();
        }
    }

    /// <summary>
    /// Add an away player to a match using the sp_set_match_player stored procedure
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="playerId">The ID of the away player to add</param>
    /// <param name="tournamentId">The ID of the tournament</param>
    /// <returns>The ID of the created match_player record</returns>
    public async Task<int> AddAwayPlayer(int matchId, int playerId, int tournamentId)
    {
        _dbconn.Open();

        try
        {
            var command = _dbconn.CreateCommandStoreProc("sp_set_match_player",
                new[] { "p_id", "p_match", "p_player_home", "p_player_away", "p_tournament" },
                new object[] { DBNull.Value, matchId, DBNull.Value, playerId, tournamentId });

            var result = await command.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }
        finally
        {
            _dbconn.Close();
        }
    }

    /// <summary>
    /// Add multiple home players to a match using the sp_set_match_player stored procedure
    /// </summary>
    /// <param name="matchId">The ID of the match</param>
    /// <param name="playerIds">The IDs of the home players to add</param>
    /// <param name="tournamentId">The ID of the tournament</param>
    /// <returns>List of IDs of the created match_player records</returns>
    public async Task<List<int>> AddHomePlayers(int matchId, List<int> playerIds, int tournamentId)
    {
        var recordIds = new List<int>();
        _dbconn.Open();

        try
        {
            foreach (var playerId in playerIds)
            {
                var command = _dbconn.CreateCommandStoreProc("sp_set_match_player",
                    new[] { "p_id", "p_match", "p_player_home", "p_player_away", "p_tournament" },
                    new object[] { DBNull.Value, matchId, playerId, DBNull.Value, tournamentId });

                var result = await command.ExecuteScalarAsync();
                var recordId = result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
                recordIds.Add(recordId);
            }
        }
        finally
        {
            _dbconn.Close();
        }

        return recordIds;
    }
}