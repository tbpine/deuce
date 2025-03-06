using System.Data.Common;
using System.Diagnostics;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Runtime.Versioning;
using deuce.ext;

namespace deuce;
/// <summary>
/// Get a list of teams for a club
/// </summary>
public class DbRepoTeam : DbRepoBase<Team>
{
    /// <summary>
    /// Used to delete players
    /// from a team.
    /// </summary>
    private record TeamPlayer(Player Player, int TeamId);

    public Organization? Organization { get; set; }
    public int TournamentId { get=>_tournamentId; set=>_tournamentId = value; }

    private int _tournamentId;

    /// <summary>
    /// Construct with a db connection
    /// </summary>
    /// <param name="dbconn">Db connection</param>
    public DbRepoTeam(DbConnection dbconn) : base(dbconn)
    {
    }

    /// <summary>
    /// Construct with a db connection
    /// </summary>
    /// <param name="dbconn">Db connection</param>
    /// <param name="organization">Organization</param>
    /// <param name="tournamentId">Tournament id</param>
    public DbRepoTeam(DbConnection dbconn, Organization organization, int tournamentId) : base(dbconn)
    {
        Organization = organization;
        _tournamentId = tournamentId;
    }

    public override async Task<List<Team>> GetList(Filter filter)
    {
        _dbconn.Open();

        List<Team> result = new();

        using (DbCommand cmd = _dbconn.CreateCommand())
        {
            cmd.CommandText = "sp_get_team";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add(cmd.CreateWithValue("p_organization", filter.ClubId));

            var reader = new SafeDataReader(await cmd.ExecuteReaderAsync());

            while (reader.Target.Read())
            {
                Team team = new()
                {
                    Id = reader.Target.Parse<int>("id"),
                    Label = reader.Target.Parse<string>("label")
                };
                team.Club = Organization;

                result.Add(team);
            }
        }

        _dbconn.Close();

        return result;

    }

    /// <summary>
    /// Save a team to the database
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override async Task SetAsync(Team obj)
    {
        _dbconn.Open();

        var localtran = _dbconn.BeginTransaction();

        try
        {
            //Explicitly use a DBNULL in the id column for new teams
            object primaryKeyId = obj.Id < 1 ? DBNull.Value : obj.Id;

            DbCommand cmdSetTeam = _dbconn.CreateCommandStoreProc("sp_set_team",
            ["p_id", "p_organization", "p_tournament", "p_label", "p_index"],
            [primaryKeyId, Organization?.Id ?? 1, _tournamentId, obj.Label, obj.Index],
            localtran);

            object? id = null;
            id = await cmdSetTeam.ExecuteScalarAsync();
            obj.Id = cmdSetTeam.GetIntegerFromScaler(id);

            //Save team players
            foreach (Player player in obj.Players)
            {
                primaryKeyId = player.TeamPlayerId < 1 ? DBNull.Value : player.TeamPlayerId;

                DbCommand cmdSetTeamPlayer = _dbconn.CreateCommandStoreProc("sp_set_team_player",
                ["p_id", "p_team", "p_player", "p_player_first", "p_player_last", "p_tournament", "p_organization", "p_index"],
                [ primaryKeyId , obj.Id, player.Id > 0 ? player.Id : DBNull.Value, string.IsNullOrEmpty(player.First) ? DBNull.Value : player.First,
                string.IsNullOrEmpty(player.Last) ? DBNull.Value : player.Last,  _tournamentId, Organization?.Id ?? 1,player.Index],
                localtran);

                await cmdSetTeamPlayer.ExecuteNonQueryAsync();

            }

            localtran.Commit();


        }
        catch (DbException ex)
        {
            localtran.Rollback();
            Debug.WriteLine(ex.Message);
        }
        finally
        {
            _dbconn.Close();
        }


    }

    /// <summary>
    /// Save a team to the database
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override void Set(Team obj)
    {
        var localtran = _dbconn.BeginTransaction();

        try
        {
            //Explicitly use a DBNULL in the id column for new teams
            object primaryKeyId = obj.Id < 1 ? DBNull.Value : obj.Id;

            DbCommand cmdSetTeam = _dbconn.CreateCommandStoreProc("sp_set_team",
            ["p_id", "p_organization", "p_tournament", "p_label", "p_index"],
            [primaryKeyId, Organization?.Id ?? 1, _tournamentId, obj.Label, obj.Index],
            localtran);

            object? id = null;
            id = cmdSetTeam.ExecuteScalar();
            obj.Id = cmdSetTeam.GetIntegerFromScaler(id);

            //Save team players
            foreach (Player player in obj.Players)
            {
                primaryKeyId = player.TeamPlayerId < 1 ? DBNull.Value : player.TeamPlayerId;

                DbCommand cmdSetTeamPlayer = _dbconn.CreateCommandStoreProc("sp_set_team_player",
                ["p_id", "p_team", "p_player", "p_player_first", "p_player_last", "p_tournament", "p_organization", "p_index"],
                [ primaryKeyId , obj.Id, player.Id, string.IsNullOrEmpty(player.First) ? DBNull.Value : player.First,
                string.IsNullOrEmpty(player.Last) ? DBNull.Value : player.Last,  _tournamentId, Organization?.Id ?? 1,player.Index],
                localtran);

                cmdSetTeamPlayer.ExecuteNonQuery();

            }

            localtran.Commit();


        }
        catch (DbException ex)
        {
            localtran.Rollback();
            Debug.WriteLine(ex.Message);
        }

    }

    public override async Task Sync(List<Team> src)
    {
        _dbconn.Open();

        //Get a list of destination records
        DbRepoRecordTeamPlayer dbRepoTeamPlayer = new(_dbconn);
        Filter filterTeams = new Filter() { ClubId = Organization?.Id ?? 1, TournamentId = _tournamentId };
        var teamplayers = await dbRepoTeamPlayer.GetList(filterTeams);

        TeamRepo teamRepo = new(teamplayers);
        var dest = teamRepo.ExtractFromRecordTeamPlayer();

        SyncMaster<Team> syncTeam = new(src, dest);

        //Use lists to sore
        //pending changes.
        List<Team> addList = new();
        List<Team> updateList = new();
        List<Team> delList = new();

        List<TeamPlayer> delTeamPlayer = new();
        List<TeamPlayer> addTeamPlayer = new();

        //Do everything in memory first
        //, then apply changes to the 
        //database using transactions. This
        //stops context switching between
        //anonymous functions.

        syncTeam.Add += (s, e) =>
        {
            //Players are added 
            addList.Add(e);
        };
        syncTeam.Update += (s, e) =>
        {
            if (e.Source is not null && e.Dest is not null)
            {
                updateList.Add(e.Source);
                //Sync players list
                var srcPlayers = new List<Player>(e.Source.Players);
                var destPlayers = new List<Player>(e.Dest.Players);
                SyncMaster<Player> syncPlayer = new(srcPlayers, destPlayers);
                int srcTeamId = e.Source.Id;

                syncPlayer.Add += (s, e) =>
                {
                    addTeamPlayer.Add(new TeamPlayer(e, srcTeamId));

                };

                syncPlayer.Remove += (s, e) =>
                {
                    delTeamPlayer.Add(new TeamPlayer(e, srcTeamId));
                };

                syncPlayer.Run();

            }

        };
        syncTeam.Remove += (s, e) =>
        {
            delList.Add(e);
            //Remove players as well

        };

        syncTeam.Run();
        //------------------------------------
        //| Apply DB Changes                 |
        //------------------------------------

        DbTransaction localtran = _dbconn.BeginTransaction();

        try
        {
            //Add new teams
            foreach (Team team in addList) InsertTeam(team, localtran);
            //Update existing teams
            foreach (Team team in updateList) UpdateTeamDetail(team, localtran);
            //Remove teams 
            foreach (Team team in delList) DeleteTeam(team, localtran);

            //Team player relatoinships 

            //Add new team players
            foreach (TeamPlayer teamPlayer in addTeamPlayer)
            {
                InsertTeamPlayer(teamPlayer.TeamId, teamPlayer.Player.Id, teamPlayer.Player.First ?? "",
                teamPlayer.Player.Last ?? "", _tournamentId, Organization?.Id??1, teamPlayer.Player.Index, localtran);
            }

            //Remove players from a team

            foreach (TeamPlayer teamPlayer in delTeamPlayer)
            {
                DeleteTeamPlayer(teamPlayer.TeamId, teamPlayer.Player.Id, _tournamentId, localtran);
            }


            localtran.Commit();
        }
        catch (Exception ex)
        {
            localtran.Rollback();
            Debug.WriteLine(ex.ToString());
        }
        finally
        {
            _dbconn.Close();
        }

        //Deletes
    }

    /// <summary>
    /// Insert rows into the team , team_player table.
    /// </summary>
    /// <param name="team">Team to save</param>
    /// <param name="dbTransaction">Transaction if any</param>
    private void InsertTeam(Team team, DbTransaction dbTransaction)
    {
        //Update team details.

        var cmdSetTeam = _dbconn.CreateCommandStoreProc("sp_set_team", ["p_id", "p_organization", "p_tournament", "p_label", "p_index"]
        , [DBNull.Value, Organization?.Id ?? 1, _tournamentId, team.Label, team.Index], dbTransaction);

        //  A new row is inserted. Store the id.
        team.Id = cmdSetTeam.GetIntegerFromScaler(cmdSetTeam.ExecuteScalar());


        foreach (Player player in team.Players)
        {
            //set team players
            DbCommand cmdSetTeamPlayer = _dbconn.CreateCommandStoreProc("sp_set_team_player",
            ["p_id", "p_team", "p_player", "p_player_first", "p_player_last", "p_tournament", "p_organization", "p_index"],
            [ DBNull.Value, team.Id, player.Id, string.IsNullOrEmpty(player.First) ? DBNull.Value : player.First,
                string.IsNullOrEmpty(player.Last) ? DBNull.Value : player.Last,  _tournamentId, Organization?.Id??1,player.Index],
            dbTransaction);


            var reader = cmdSetTeamPlayer.ExecuteReader();
            if (reader.Read()) player.Id = reader.GetInt32(reader.GetOrdinal("player_id"));
            reader.Close();

        }

    }

    /// <summary>
    /// Update a row in the team table
    /// </summary>
    /// <param name="team">Team to update</param>
    /// <param name="dbTransaction">Transaction if any</param>
    private void UpdateTeamDetail(Team team, DbTransaction dbTransaction)
    {
        //Update team details.
        var cmdSetTeam = _dbconn.CreateCommandStoreProc("sp_set_team", ["p_id", "p_organization", "p_tournament", "p_label", "p_index"]
        , [team.Id, Organization?.Id ?? 1, _tournamentId, team.Label, team.Index], dbTransaction);

        var teamScalerId = cmdSetTeam.ExecuteScalar();
        //Returns the last inserted row id
        //or updated row id

        team.Id = cmdSetTeam.GetIntegerFromScaler(teamScalerId);

    }

    private void DeleteTeam(Team team, DbTransaction dbTransaction)
    {
        //Delete a team
        var cmdDeleteTeam = _dbconn.CreateCommandStoreProc("sp_delete_team", ["p_id"]
        , [team.Id], dbTransaction);
        //Update team details.
        cmdDeleteTeam.ExecuteNonQuery();
    }


    private void DeleteTeamPlayer(int teamId, int playerId, int tournamentId, DbTransaction tran)
    {
        var cmdDeleteTeamPlayer = _dbconn.CreateCommandStoreProc("sp_delete_team_player", ["p_team", "p_player", "p_tournament"]
       , [teamId, playerId, tournamentId], tran);
        cmdDeleteTeamPlayer.ExecuteNonQuery();

    }

    private void InsertTeamPlayer(int teamId, int playerId, string first, string last, int tournamentId,
    int orgId, int pIndex, DbTransaction tran)
    {

        DbCommand cmdSetTeamPlayer = _dbconn.CreateCommandStoreProc("sp_set_team_player",
        ["p_id", "p_team", "p_player", "p_player_first", "p_player_last", "p_tournament", "p_organization", "p_index"],
        [DBNull.Value, teamId, playerId, first, last, tournamentId, orgId, pIndex],
        tran);

        var reader = cmdSetTeamPlayer.ExecuteReader();
        if (reader.Read()) Debug.WriteLine(reader.GetInt32(reader.GetOrdinal("player_id")));
        reader.Close();

    }

}