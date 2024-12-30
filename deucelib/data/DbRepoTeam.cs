using System.Data.Common;
using deuce.ext;

namespace deuce;
/// <summary>
/// Get a list of teams for a club
/// </summary>
public class DbRepoTeam : DbRepoBase<Team>
{
    private readonly DbConnection _dbconn;
    private readonly Club? _club;
    

    /// <summary>
    /// Construct with a db connection
    /// </summary>
    /// <param name="dbconn"></param>
    public DbRepoTeam (DbConnection dbconn, params object[] references)
    {
        _dbconn = dbconn;   
        _club = references[0] as Club;
    }

    public override async Task<List<Team>> GetList(Filter filter)
    {
        List<Team> result=new();

        using (DbCommand cmd = _dbconn.CreateCommand())
        {
            cmd.CommandText = "sp_get_team";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add(cmd.CreateWithValue("p_club", filter.ClubId));
            
            var reader = await cmd.ExecuteReaderAsync();
            
            while (reader.Read())
            {
                Team team = new(){
                    Id = reader.Parse<int>("id"),
                    Label = reader.Parse<string>("label")
                };
                team.Club = _club;

                result.Add(team);
            }
        }

        return result;  

    }

}