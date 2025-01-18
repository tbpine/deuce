namespace deuce;
public class DbRepoBase<T> : IDbRepo<T>
{
    public virtual Task<List<T>> GetList()
    {
        return Task.FromResult(new List<T>());
    }

    public virtual Task<List<T>> GetList(Filter filter)
    {
        return Task.FromResult(new List<T>());
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

    public virtual Task Sync(List<T> obj, Filter filter)
    {
        return Task.FromResult(obj);
    }

}