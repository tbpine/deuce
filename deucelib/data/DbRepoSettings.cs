namespace deuce;
using System.Data.Common;

using deuce.ext;

public class DbRepoSettings : DbRepoBase<Setting>
{
    private readonly DbConnection _dbconn;

    /// <summary>
    /// Construct with dependency
    /// </summary>
    /// <param name="dbconn"></param>
    public DbRepoSettings(DbConnection dbconn)
    {
        _dbconn = dbconn;
    }

    /// <summary>
    /// Get sports
    /// </summary>
    /// <returns></returns>
    public async override Task<List<Setting>> GetList()
    {
        List<Setting> listOfSettings = new();
        await _dbconn.CreateReaderStoreProcAsync("sp_get_settings", [], [], reader=>{
            listOfSettings.Add(new Setting(reader.Parse<int>("id"), 
                reader.Parse<string>("key"),
                reader.Parse<string>("value")));

        });

        return listOfSettings;
    }
}