/// <summary>
/// Defines caching manangment
/// </summary>
public interface ICacheMaster
{
    Task<double> GetDouble(string key);
    Task<List<T>?> GetList<T>(string key);
}