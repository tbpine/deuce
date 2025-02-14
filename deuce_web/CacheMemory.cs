
/// <summary>
/// Remember values in a dictionary.
/// </summary>
public class CacheMemory : ICache
{
    /// <summary>
    /// Underlying cache structure
    /// </summary>
    private Dictionary<string, object> _cache = new Dictionary<string,object>();

    /// <summary>
    /// Add a value
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">Object to store</param>
    public void Add(string key, object value)
    {
        
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public object GetValue(string key)
    {
        throw new NotImplementedException();
    }
}