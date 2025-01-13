using System.Data.Common;
using System.Runtime.CompilerServices;
using deuce.ext;

namespace deuce;
public class DbRepoSport : DbRepoBase<Sport>
{
    private readonly DbConnection _dbconn;

    /// <summary>
    /// Construct with dependency
    /// </summary>
    /// <param name="dbconn"></param>
    public DbRepoSport(DbConnection dbconn)
    {
        _dbconn = dbconn;
    }

    /// <summary>
    /// Get sports
    /// </summary>
    /// <returns></returns>
    public async override Task<List<Sport>> GetList()
    {
        List<Sport> sports = new();

        //Insert into the team table
        using (DbCommand cmd = _dbconn.CreateCommand())
        {
            cmd.CommandText = "sp_get_sports";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            var reader = new SafeDataReader(await cmd.ExecuteReaderAsync());
            
            while (reader.Target.Read())
            {
                sports.Add(new Sport(reader.Target.Parse<int>("id"), reader.Target.Parse<string>("label"),
                reader.Target.Parse<string>("name"),
                reader.Target.Parse<string>("key"),
                reader.Target.Parse<string>("icon")));
            }

            reader.Target.Close();

        }

        return sports;
    }
}