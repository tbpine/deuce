using System.Data.Common;

namespace deuce;

public interface IDbRepo<T>
{
    Task<List<T>> GetList();
    Task<List<T>> GetList(Filter filter);
    Task Set(T obj);
}