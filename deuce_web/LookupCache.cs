
using System.Security;
using System.Threading.Tasks;
using deuce;

public class LookupCache : ILookup
{
    private readonly ICacheMaster _cache;
    
    /// <summary>
    /// Construct with a cache
    /// </summary>
    /// <param name="cacheMaster"></param>
    public LookupCache(ICacheMaster cacheMaster) => _cache = cacheMaster;

    /// <summary>
    /// Given the type, look up it's label
    /// </summary>
    /// <param name="value">Usually an index</param>
    /// <param name="lookupType">Type to look up</param>
    /// <returns>Label </returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<string?> GetLabel(object value, Type lookupType)
    {
        //Switch on the lookup type.
        if (lookupType == typeof(Interval))
        {
            //Use the cached value
            List<Interval>? intervals = await _cache.GetList<Interval>(CacheMasterDefault.KEY_INTERVALS);
            if (intervals is not null)
                return intervals.Find(e=>e.Id == (int)value)?.Label;
        }

        return "";
    }
}