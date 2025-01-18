using System.Data.Common;
using System.Diagnostics;
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
    private readonly DbConnection _dbconn;

    public Organization? Club { get; set; }
    public Tournament? Tournament { get; set; }


    private int _tournamentId;
    private bool _ignoreEmptyPlayers;

    /// <summary>
    /// Construct with a db connection
    /// </summary>
    /// <param name="dbconn">Db connection</param>
    public DbRepoTeam(DbConnection dbconn)
    {
        _dbconn = dbconn;
    }

    /// <summary>
    /// Construct with a db connection
    /// </summary>
    /// <param name="dbconn">Db connection</param>
    /// <param name="organization">Organization</param>
    /// <param name="tournamentId">Tournament id</param>
    /// <param name="ignoreEmptyPlayers">true to save players with no name</param>
    public DbRepoTeam(DbConnection dbconn, Organization organization, int tournamentId, bool ignoreEmptyPlayers)
    {
        _dbconn = dbconn;
        Club = organization;
        _tournamentId = tournamentId;
        _ignoreEmptyPlayers = ignoreEmptyPlayers;
    }

    public override async Task<List<Team>> GetList(Filter filter)
    {
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
                team.Club = Club;

                result.Add(team);
            }
        }

        return result;

    }

    /// <summary>
    /// Save a team to the database
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override async Task SetAsync(Team obj)
    {
        var localtran = _dbconn.BeginTransaction();

        try
        {

            //Insert into the team table
            DbCommand cmd = _dbconn.CreateCommand();
            cmd.Transaction = localtran;
            cmd.CommandText = "sp_set_team";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add(cmd.CreateWithValue("p_id", obj.Id > 0 ? obj.Id : DBNull.Value));
            cmd.Parameters.Add(cmd.CreateWithValue("p_organization", Club?.Id ?? 1));
            cmd.Parameters.Add(cmd.CreateWithValue("p_tournament", _tournamentId));
            cmd.Parameters.Add(cmd.CreateWithValue("p_label", obj.Label));
            cmd.Parameters.Add(cmd.CreateWithValue("p_index", obj.Index));

            object? id = null;
            id = await cmd.ExecuteScalarAsync();

            obj.Id = cmd.GetIntegerFromScaler(id);

            cmd = _dbconn.CreateCommand();
            cmd.Transaction = localtran;
            cmd.CommandText = "sp_set_team_player";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.Add(cmd.CreateWithValue("p_id", obj.Id));
            cmd.Parameters.Add(cmd.CreateWithValue("p_team", obj.Id));
            cmd.Parameters.Add(cmd.CreateWithValue("p_player", -1));
            cmd.Parameters.Add(cmd.CreateWithValue("p_player_first", -1));
            cmd.Parameters.Add(cmd.CreateWithValue("p_player_last", -1));
            cmd.Parameters.Add(cmd.CreateWithValue("p_tournament", -1));
            cmd.Parameters.Add(cmd.CreateWithValue("p_organization", -1));
            cmd.Parameters.Add(cmd.CreateWithValue("p_index", -1));


            //Save team players
            foreach (Player player in obj.Players)
            {
                //Don't save new players with no names
                if (!player.IsStorable()) continue;

                cmd.Parameters["p_id"].Value = player.TeamPlayerId > 0 ? player.TeamPlayerId : DBNull.Value;
                cmd.Parameters["p_team"].Value = obj.Id;
                cmd.Parameters["p_player"].Value = player.Id;
                cmd.Parameters["p_player_first"].Value = string.IsNullOrEmpty(player.First) ? DBNull.Value : player.First;
                cmd.Parameters["p_player_last"].Value = string.IsNullOrEmpty(player.Last) ? DBNull.Value : player.Last;
                cmd.Parameters["p_tournament"].Value = _tournamentId;
                cmd.Parameters["p_organization"].Value = Club?.Id ?? 1;
                cmd.Parameters["p_index"].Value = player.Index;

                await cmd.ExecuteNonQueryAsync();


            }

            localtran.Commit();




        }
        catch (DbException ex)
        {
            localtran.Rollback();
            Debug.WriteLine(ex.Message);
        }

    }

    public override async Task Sync(List<Team> src, Filter filter)
    {
        //Get a list of destination records
        DbRepoRecordTeamPlayer dbRepoTeamPlayer = new(_dbconn);
        var teamplayers = await dbRepoTeamPlayer.GetList(filter);

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
            foreach (Team team in addList) UpdateTeam(team, filter, localtran);
            //Update existing teams
            foreach (Team team in updateList) UpdateTeamDetail(team, filter, localtran);
            //Remove teams 
            foreach (Team team in delList) DeleteTeam(team, localtran);

            //Team player relatoinships 

            //Add new team players
            foreach (TeamPlayer teamPlayer in addTeamPlayer)
            {
                InsertTeamPlayer(teamPlayer.TeamId, teamPlayer.Player.Id, teamPlayer.Player.First ?? "",
                teamPlayer.Player.Last ?? "", filter.TournamentId, filter.ClubId, teamPlayer.Player.Index, localtran);
            }

            //Remove players from a team

            foreach (TeamPlayer teamPlayer in delTeamPlayer)
            {
                DeleteTeamPlayer(teamPlayer.TeamId, teamPlayer.Player.Id, filter.TournamentId, localtran);
            }


               localtran.Commit();
        }
        catch (Exception ex)
        {
             localtran.Rollback();
            Debug.WriteLine(ex.ToString());
        }



        //Deletes
    }

    private void UpdateTeam(Team team, Filter filter, DbTransaction dbTransaction)
    {
        //Update team details.
        var cmd = _dbconn.CreateCommand();
        cmd.CommandText = "sp_set_team";
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.Parameters.Add(cmd.CreateWithValue("p_id", team.Id < 1 ? DBNull.Value : team.Id));
        cmd.Parameters.Add(cmd.CreateWithValue("p_organization", filter.ClubId));
        cmd.Parameters.Add(cmd.CreateWithValue("p_tournament", filter.TournamentId));
        cmd.Parameters.Add(cmd.CreateWithValue("p_label", team.Label));
        cmd.Parameters.Add(cmd.CreateWithValue("p_index", team.Index));
        cmd.Transaction = dbTransaction;

        //  A new row is inserted. Store the id.
        team.Id = cmd.GetIntegerFromScaler(cmd.ExecuteScalar());



        //set team players
        var cmd2 = _dbconn.CreateCommand();
        cmd2.CommandText = "sp_set_team_player";
        cmd2.CommandType = System.Data.CommandType.StoredProcedure;
        cmd2.Parameters.Add(cmd.CreateWithValue("p_id", DBNull.Value));
        cmd2.Parameters.Add(cmd.CreateWithValue("p_team", 0));
        cmd2.Parameters.Add(cmd.CreateWithValue("p_player", 0));
        cmd2.Parameters.Add(cmd.CreateWithValue("p_player_first", ""));
        cmd2.Parameters.Add(cmd.CreateWithValue("p_player_last", ""));
        cmd2.Parameters.Add(cmd.CreateWithValue("p_tournament", 0));
        cmd2.Parameters.Add(cmd.CreateWithValue("p_organization", 0));
        cmd2.Parameters.Add(cmd.CreateWithValue("p_index", 0));
        cmd2.Transaction = dbTransaction;

        foreach (Player player in team.Players)
        {
            cmd2.Parameters["p_id"].Value = DBNull.Value;
            cmd2.Parameters["p_team"].Value = team.Id;
            cmd2.Parameters["p_player"].Value = player.Id;
            cmd2.Parameters["p_player_first"].Value = player.First;
            cmd2.Parameters["p_player_last"].Value = player.Last;
            cmd2.Parameters["p_tournament"].Value = filter.TournamentId;
            cmd2.Parameters["p_organization"].Value = filter.ClubId;
            cmd2.Parameters["p_index"].Value = player.Index;


            var reader = cmd2.ExecuteReader();
            if (reader.Read()) player.Id = reader.GetInt32(reader.GetOrdinal("player_id"));
            reader.Close();

        }

    }

    public void UpdateTeamDetail(Team team, Filter filter, DbTransaction dbTransaction)
    {
        //Update team details.
        var cmd = _dbconn.CreateCommand();
        cmd.CommandText = "sp_set_team";
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.Parameters.Add(cmd.CreateWithValue("p_id", team.Id < 1 ? DBNull.Value : team.Id));
        cmd.Parameters.Add(cmd.CreateWithValue("p_organization", filter.ClubId));
        cmd.Parameters.Add(cmd.CreateWithValue("p_tournament", filter.TournamentId));
        cmd.Parameters.Add(cmd.CreateWithValue("p_label", team.Label));
        cmd.Parameters.Add(cmd.CreateWithValue("p_index", team.Index));
        cmd.Transaction = dbTransaction;

        var teamScalerId = cmd.ExecuteScalar();
        //Returns the last inserted row id
        //or updated row id

        team.Id = cmd.GetIntegerFromScaler(teamScalerId);

    }

    private void DeleteTeam(Team team, DbTransaction dbTransaction)
    {
        //Update team details.
        var cmd = _dbconn.CreateCommand();
        cmd.CommandText = "sp_delete_team";
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.Parameters.Add(cmd.CreateWithValue("p_id", team.Id));
        cmd.Transaction = dbTransaction;
        cmd.ExecuteNonQuery();
    }


    private void DeleteTeamPlayer(int teamId, int playerId, int tournamentId, DbTransaction tran)
    {
        //Update team details.
        var cmd = _dbconn.CreateCommand();
        cmd.CommandText = "sp_delete_team_player";
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.Parameters.Add(cmd.CreateWithValue("p_team", teamId));
        cmd.Parameters.Add(cmd.CreateWithValue("p_player", playerId));
        cmd.Parameters.Add(cmd.CreateWithValue("p_tournament", tournamentId));
        cmd.Transaction = tran;
        cmd.ExecuteNonQuery();

    }

    private void InsertTeamPlayer(int teamId, int playerId, string first, string last, int tournamentId,
    int orgId, int pIndex, DbTransaction tran)
    {

        //set team players
        var cmd2 = _dbconn.CreateCommand();
        cmd2.CommandText = "sp_set_team_player";
        cmd2.CommandType = System.Data.CommandType.StoredProcedure;
        cmd2.Parameters.Add(cmd2.CreateWithValue("p_id", DBNull.Value));
        cmd2.Parameters.Add(cmd2.CreateWithValue("p_team", teamId));
        cmd2.Parameters.Add(cmd2.CreateWithValue("p_player", playerId));
        cmd2.Parameters.Add(cmd2.CreateWithValue("p_player_first", first));
        cmd2.Parameters.Add(cmd2.CreateWithValue("p_player_last", last));
        cmd2.Parameters.Add(cmd2.CreateWithValue("p_tournament", tournamentId));
        cmd2.Parameters.Add(cmd2.CreateWithValue("p_organization", orgId));
        cmd2.Parameters.Add(cmd2.CreateWithValue("p_index", pIndex));
        cmd2.Transaction = tran;
        
        var reader = cmd2.ExecuteReader();
        if (reader.Read()) Debug.WriteLine(reader.GetInt32(reader.GetOrdinal("player_id")));
        reader.Close();

    }

}