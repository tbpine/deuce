/// <summary>
/// Defines methods for caching values.
/// </summary>
public interface ICache
{
    void Add(string key, object value);
    object GetValue(string key);
    void Clear();
}