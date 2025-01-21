using System.Data.Common;

namespace deuce;

public interface IDbRepo<T>
{
    Task<List<T>> GetList();
    Task<List<T>> GetList(Filter filter);
    Task SetAsync(T obj);
    void Set(T obj);
    Task Delete(T obj);
    Task Sync(List<T> obj);
}