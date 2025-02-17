using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using deuce;
using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Manages cached values
/// </summary>
public class CacheMasterDefault : ICacheMaster
{
    private readonly IMemoryCache _memoryCache;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Add the memory cache singleton
    /// </summary>
    /// <param name="memoryCache"></param>
    public CacheMasterDefault(IMemoryCache memoryCache, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _memoryCache = memoryCache;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    /// <summary>
    /// Get a cached double value
    /// </summary>
    /// <param name="key">key</param>
    /// <returns>0d if the key does not exists</returns>
    public async Task<double> GetDouble(string key)
    {
        //Select values if it doesn't existing
        if (!_memoryCache.TryGetValue(key, out _)) await SelectValuesFromDB();

        object? cacheValue = _memoryCache.TryGetValue(key, out cacheValue)? cacheValue : 0d;
        
        return Convert.ToDouble(cacheValue);
    }

    /// <summary>
    /// Select settings from the database 
    /// </summary>
    private async Task SelectValuesFromDB()
    {
        using var scope = _serviceProvider.CreateScope();
        using var dbconn = scope.ServiceProvider.GetService<DbConnection>();
        //Couldn't create db handle.
        if (dbconn == null) return;

        dbconn.ConnectionString = _configuration.GetConnectionString("deuce_local");
        dbconn.Open();

        DbRepoSettings dbRepoSettings = new(dbconn);
        var listOfSettings = await dbRepoSettings.GetList();

        //Insert all settings into the cache
        //Has to be strings since it came from
        //the settings table.
        foreach(Setting setting in listOfSettings )
        {
            //Store values for non empty keys
            if (!String.IsNullOrEmpty(setting.key) && !_memoryCache.TryGetValue(setting.key, out _))
                _memoryCache.Set<string>(setting.key, setting.value??"");
        }

    }
}