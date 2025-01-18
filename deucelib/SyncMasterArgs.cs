
public class SyncMasterArgs<T> where T : class, new()
{
    private readonly T? _source;
    private readonly T? _dest;

    public T? Source { get=>_source; }
    public T? Dest { get=>_dest; }


    public SyncMasterArgs(T source, T dest)
    {
        _source = source;
        _dest = dest;
    }

}