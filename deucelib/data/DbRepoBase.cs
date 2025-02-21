using System.Data.Common;

namespace deuce;
public class DbRepoBase<T> : IDbRepo<T>, IDisposable
{
    protected readonly DbConnection _dbconn;

    public DbRepoBase(DbConnection dbconn)
    {
        _dbconn = dbconn;
    }
    public virtual async Task<List<T>> GetList()
    {
        return await Task.FromResult(new List<T>());
    }

    public virtual async  Task<List<T>> GetList(Filter filter)
    {
        return await  Task.FromResult(new List<T>());
    }

    public virtual Task SetAsync(T obj)
    {
        return Task.FromResult(obj);
    }

    
    public virtual void Set(T obj)
    {
        
    }


    public virtual Task Delete(T obj)
    {
        return Task.FromResult(obj);
    }

    public virtual Task Sync(List<T> obj)
    {
        return Task.FromResult(obj);
    }

    public void Dispose()
    {
        _dbconn?.Close();       
    }
}