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

    public virtual Task Set(T obj)
    {
        return Task.FromResult(obj);
    }
}