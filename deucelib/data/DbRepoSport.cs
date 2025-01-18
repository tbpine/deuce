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
        await _dbconn.CreateReaderStoreProcAsync("sp_get_sports", [""], [], reader=>{
            sports.Add(new Sport(reader.Parse<int>("id"), reader.Parse<string>("label"),
                reader.Parse<string>("name"),
                reader.Parse<string>("key"),
                reader.Parse<string>("icon")));

        });

        return sports;
    }
}