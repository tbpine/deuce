using System.Data.Common;
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

    /// <summary>
    /// Construct with a db connection
    /// </summary>
    /// <param name="dbconn">Db connection</param>
    public DbRepoTeam(DbConnection dbconn)
    {
        _dbconn = dbconn;
    }

    public override async Task<List<Team>> GetList(Filter filter)
    {
        List<Team> result = new();

        using (DbCommand cmd = _dbconn.CreateCommand())
        {
            cmd.CommandText = "sp_get_team";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add(cmd.CreateWithValue("p_club", filter.ClubId));

            var reader = await cmd.ExecuteReaderAsync();

            while (reader.Read())
            {
                Team team = new()
                {
                    Id = reader.Parse<int>("id"),
                    Label = reader.Parse<string>("label")
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
        //Insert into the team table
        using (DbCommand cmd = _dbconn.CreateCommand())
        {
            cmd.CommandText = "sp_set_team";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add(cmd.CreateWithValue("p_id", obj.Id > 0 ? obj.Id : DBNull.Value));
            cmd.Parameters.Add(cmd.CreateWithValue("p_club", Club?.Id ?? -1));
            cmd.Parameters.Add(cmd.CreateWithValue("p_tournament", Tournament?.Id ?? -1));
            cmd.Parameters.Add(cmd.CreateWithValue("p_label", obj.Label));

            object? id = await cmd.ExecuteScalarAsync();

            obj.Id = (int)(ulong)(id ?? 0L);


        }
        using (DbCommand cmd = _dbconn.CreateCommand())
        {
            cmd.CommandText = "sp_set_team_player";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add(cmd.CreateWithValue("p_team", obj.Id ));
            cmd.Parameters.Add(cmd.CreateWithValue("p_player", -1));
            cmd.Parameters.Add(cmd.CreateWithValue("p_tournament", -1));

            //Save team players
            foreach (Player player in obj.Players)
            {
                cmd.Parameters["p_team"].Value = obj.Id;
                cmd.Parameters["p_player"].Value = player.Id;
                cmd.Parameters["p_tournament"].Value = Tournament?.Id??-1;

                await cmd.ExecuteNonQueryAsync();

                
            }
        }
    }
}