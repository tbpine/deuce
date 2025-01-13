using System.Data.Common;
using System.Diagnostics;
using deuce.ext;

namespace deuce;
/// <summary>
/// Get a list of teams for a club
/// </summary>
public class DbRepoTeam : DbRepoBase<Team>
{
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
    public override async Task Set(Team obj)
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

            object? id = null;
            id = await cmd.ExecuteScalarAsync();

            obj.Id = (int)(ulong)(id ?? 0L);

            cmd = _dbconn.CreateCommand();
            cmd.Transaction = localtran;
            cmd.CommandText = "sp_set_team_player";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add(cmd.CreateWithValue("p_team", obj.Id));
            cmd.Parameters.Add(cmd.CreateWithValue("p_player", -1));
            cmd.Parameters.Add(cmd.CreateWithValue("p_player_first", -1));
            cmd.Parameters.Add(cmd.CreateWithValue("p_player_last", -1));
            cmd.Parameters.Add(cmd.CreateWithValue("p_tournament", -1));
            cmd.Parameters.Add(cmd.CreateWithValue("p_organization", -1));


            //Save team players
            foreach (Player player in obj.Players)
            {
                //Don't save players with no names
                if (string.IsNullOrEmpty(player.First) ||  string.IsNullOrEmpty(player.Last)) continue;

                cmd.Parameters["p_team"].Value = obj.Id;
                cmd.Parameters["p_player"].Value = player.Id;
                cmd.Parameters["p_player_first"].Value = string.IsNullOrEmpty(player.First) ? DBNull.Value : player.First;
                cmd.Parameters["p_player_last"].Value = string.IsNullOrEmpty(player.Last) ? DBNull.Value : player.Last;
                cmd.Parameters["p_tournament"].Value = _tournamentId;
                cmd.Parameters["p_organization"].Value = Club?.Id ?? 1;

                await cmd.ExecuteNonQueryAsync();


            }

            localtran.Commit();
        }
        catch(DbException ex)  
        {
            localtran.Rollback();
            Debug.WriteLine(ex.Message);
        }

    }
}