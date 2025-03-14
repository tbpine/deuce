using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using deuce;
using Google.Protobuf.Collections;
using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Manages cached values
/// </summary>
public class CacheMasterDefault : ICacheMaster
{
    private readonly IMemoryCache _memoryCache;
    private readonly DbConnection _dbconn;

    public const String KEY_INTERVALS = "intervals";
    public const String KEY_SPORTS = "sports";
    public const String KEY_TOURNAMENT_TYPES = "tournament_types";
    public const String KEY_ENTRY_TYPES = "entry_types";
    public const String KEY_ENTRY_COUNTRIES= "countries";

    /// <summary>
    /// Add the memory cache singleton
    /// </summary>
    /// <param name="memoryCache"></param>
    public CacheMasterDefault(IMemoryCache memoryCache, IConfiguration configuration)
    {
        _memoryCache = memoryCache;
        _dbconn = new DbConnectionLocal(configuration);
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

        object? cacheValue = _memoryCache.TryGetValue(key, out cacheValue) ? cacheValue : 0d;

        return Convert.ToDouble(cacheValue);
    }

    /// <summary>
    /// Select settings from the database 
    /// </summary>
    private async Task SelectValuesFromDB()
    {
        _dbconn.Open();
        (_dbconn as DbConnectionLocal)?.KeepAlive(true);

        DbRepoSettings dbRepoSettings = new(_dbconn);
        var listOfSettings = await dbRepoSettings.GetList();

        //Insert all settings into the cache
        //Has to be strings since it came from
        //the settings table.
        foreach (Setting setting in listOfSettings)
        {
            //Store values for non empty keys
            if (!String.IsNullOrEmpty(setting.key) && !_memoryCache.TryGetValue(setting.key, out _))
                _memoryCache.Set<string>(setting.key, setting.value ?? "");
        }

        //Get static data like intreval, sport, entry type , type
        //TODO: method to do this async needed.

        await CacheConstants<Interval>(new DbRepoInterval(_dbconn), KEY_INTERVALS);
        await CacheConstants<Sport>(new DbRepoSport(_dbconn), KEY_SPORTS);
        await CacheConstants<TournamentType>(new DbRepoTournamentType(_dbconn), KEY_TOURNAMENT_TYPES);
        await CacheConstants<Country>(new DbRepoCountry(_dbconn), KEY_ENTRY_COUNTRIES);
        
        (_dbconn as DbConnectionLocal)?.KeepAlive(false);
        _dbconn.Close();

    }

    /// <summary>
    /// Load constants from the database asynchronously
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbRepo">The dbrepo class</param>
    /// <param name="key">cache key</param>
    /// <returns></returns>
    private async Task CacheConstants<T>(DbRepoBase<T> dbRepo, string key)
    {
        List<T> listOfTs = await dbRepo.GetList();
        if (!_memoryCache.TryGetValue<List<T>>(key, out _))
            _memoryCache.Set<List<T>>(key, listOfTs);
    }

    /// <summary>
    /// Get a list
    /// </summary>
    /// <typeparam name="T">Type of list objects to get</typeparam>
    /// <param name="key">key</param>
    /// <returns>Null if the list doesn't exist</returns>
    public async Task<List<T>?> GetList<T>(string key)
    {
        if (!_memoryCache.TryGetValue(key, out _)) await SelectValuesFromDB(); 

        return  _memoryCache.Get<List<T>>(key);
    }

}