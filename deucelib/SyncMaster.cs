using System.Data;
using System.Reflection;

public class SyncMaster<T> where T : class, new()
{
    public event EventHandler<T>? Add;
    public event EventHandler<SyncMasterArgs<T>>? Update;
    public event EventHandler<T>? Remove;

    private readonly List<T>? _source;
    private readonly List<T>? _dest;
    private readonly List<Predicate<T>>? _filters;

    /// <summary>
    /// Synchronize two collections of type T.
    /// </summary>
    /// <param name="source">Source collection to synchronize from.</param>
    /// <param name="dest">Destination collection to synchronize to.</param>
    /// <param name="filters">Optional filters to apply to the source collection.</param>
    public SyncMaster(List<T> source, List<T> dest, List<Predicate<T>>? filters = null)
    {
        _source = source;
        _dest = dest;
        _filters = filters;
    }

    public void Run()
    {
        if (_source is null || _dest is null)
            throw new ArgumentNullException("Missing source or dest collections");
        T obj = new();
        Type t = obj.GetType();
        PropertyInfo? piId = t.GetProperty("Id");

        if (piId is null) throw new ArgumentException("Type missing Id property");

        //Add /Update destination
        foreach (T srcItem in _source ?? new List<T>())
        {
            //Skip the item if it matches any of the filters
            if (Skip(srcItem)) continue;

            T? destItem = _dest.Find(e =>
            {
                int srcId = (int)(piId.GetValue(e) ?? (object)0);
                int destId = (int)(piId.GetValue(srcItem) ?? (object)1);
                return srcId == destId;
            });
            //Destination doesn't have the item
            if (destItem is null)
                //Add the it
                Add?.Invoke(this, srcItem);
            else
                //Update destination with the source item
                Update?.Invoke(this, new SyncMasterArgs<T>(srcItem, destItem));
        }

        //Remove destination elements

        foreach (T destItem in _dest)
        {
            T? srcItem = _source?.Find(e =>
            {
                int srcId = (int)(piId.GetValue(e) ?? (object)0);
                int destId = (int)(piId.GetValue(destItem) ?? (object)1);
                return srcId == destId;
            });
            if (srcItem is null) Remove?.Invoke(this, destItem);
        }

    }

    /// <summary>
    /// Skip the item if it  matches any of the filters.
    /// </summary>
    /// <param name="item">Item to check against the filters.</param>
    /// <returns>True if the item should be skipped, false otherwise.</returns>
    private bool Skip(T item)
    {
        if (_filters is null || _filters.Count == 0) return false;
        foreach (Predicate<T> filter in _filters)
        {
            if (filter(item)) return true; //Skip this item
        }
        return false; //Do not skip
    }

}