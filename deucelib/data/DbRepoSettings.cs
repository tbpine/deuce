namespace deuce;
using System.Data.Common;

using deuce.ext;

public class DbRepoSettings : DbRepoBase<Setting>
{

    /// <summary>
    /// Construct with dependency
    /// </summary>
    /// <param name="dbconn"></param>
    public DbRepoSettings(DbConnection dbconn) : base(dbconn)
    {
    }

    /// <summary>
    /// Get sports
    /// </summary>
    /// <returns></returns>
    public async override Task<List<Setting>> GetList()
    {
       _dbconn.Open();

        List<Setting> listOfSettings = new();
        await _dbconn.CreateReaderStoreProcAsync("sp_get_settings", [], [], reader=>{
            listOfSettings.Add(new Setting(reader.Parse<int>("id"), 
                reader.Parse<string>("key"),
                reader.Parse<string>("value")));

        });
        
        _dbconn.Close();

        return listOfSettings;
    }
}