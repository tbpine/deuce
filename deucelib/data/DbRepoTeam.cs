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
            DbCommand cmd = _dbconn.CreateCommandStoreProc("sp_set_team",
            ["p_id", "p_organization", "p_tournament", "p_label", "p_index"],
            [obj.Id, Club?.Id ?? 1, _tournamentId, obj.Label, obj.Index],
            localtran);

            object? id = null;
            id = await cmd.ExecuteScalarAsync();
            obj.Id = cmd.GetIntegerFromScaler(id);

            //Save team players
            foreach (Player player in obj.Players)
            {
                DbCommand cmd2 = _dbconn.CreateCommandStoreProc("sp_set_team_player",
                ["p_id", "p_team", "p_player", "p_player_first", "p_player_last", "p_tournament", "p_organization", "p_index"],
                [ player.TeamPlayerId , obj.Id, player.Id, string.IsNullOrEmpty(player.First) ? DBNull.Value : player.First,
                string.IsNullOrEmpty(player.Last) ? DBNull.Value : player.Last,  _tournamentId, Club?.Id ?? 1,player.Index],
                localtran);

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
        var cmd = _dbconn.CreateCommandStoreProc("sp_set_team", ["p_id", "p_organization", "p_tournament", "p_label", "p_index"]
        , [DBNull.Value, filter.ClubId, filter.TournamentId, team.Label, team.Index], dbTransaction);

        //  A new row is inserted. Store the id.
        team.Id = cmd.GetIntegerFromScaler(cmd.ExecuteScalar());


        foreach (Player player in team.Players)
        {
            //set team players
            DbCommand cmd2 = _dbconn.CreateCommandStoreProc("sp_set_team_player",
            ["p_id", "p_team", "p_player", "p_player_first", "p_player_last", "p_tournament", "p_organization", "p_index"],
            [ DBNull.Value, team.Id, player.Id, string.IsNullOrEmpty(player.First) ? DBNull.Value : player.First,
                string.IsNullOrEmpty(player.Last) ? DBNull.Value : player.Last,  filter.TournamentId, filter.ClubId,player.Index],
            dbTransaction);


            var reader = cmd2.ExecuteReader();
            if (reader.Read()) player.Id = reader.GetInt32(reader.GetOrdinal("player_id"));
            reader.Close();

        }

    }

    public void UpdateTeamDetail(Team team, Filter filter, DbTransaction dbTransaction)
    {
        //Update team details.
        var cmd = _dbconn.CreateCommandStoreProc("sp_set_team", ["p_id", "p_organization", "p_tournament", "p_label", "p_index"]
        , [team.Id, filter.ClubId, filter.TournamentId, team.Label, team.Index], dbTransaction);

        var teamScalerId = cmd.ExecuteScalar();
        //Returns the last inserted row id
        //or updated row id

        team.Id = cmd.GetIntegerFromScaler(teamScalerId);

    }

    private void DeleteTeam(Team team, DbTransaction dbTransaction)
    {
        //Delete a team
        var cmd = _dbconn.CreateCommandStoreProc("sp_delete_team", ["p_id"]
        , [team.Id], dbTransaction);
        //Update team details.
        cmd.ExecuteNonQuery();
    }


    private void DeleteTeamPlayer(int teamId, int playerId, int tournamentId, DbTransaction tran)
    {
        var cmd = _dbconn.CreateCommandStoreProc("sp_delete_team_player", ["p_team", "p_player", "p_tournament"]
       , [teamId, playerId, tournamentId], tran);
        cmd.ExecuteNonQuery();

    }

    private void InsertTeamPlayer(int teamId, int playerId, string first, string last, int tournamentId,
    int orgId, int pIndex, DbTransaction tran)
    {

        DbCommand cmd2 = _dbconn.CreateCommandStoreProc("sp_set_team_player",
        ["p_id", "p_team", "p_player", "p_player_first", "p_player_last", "p_tournament", "p_organization", "p_index"],
        [DBNull.Value , teamId, playerId, first, last,  tournamentId, orgId,pIndex],
        tran);

        var reader = cmd2.ExecuteReader();
        if (reader.Read()) Debug.WriteLine(reader.GetInt32(reader.GetOrdinal("player_id")));
        reader.Close();

    }

}