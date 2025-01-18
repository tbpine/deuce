using System.Data;
using System.Reflection;

public class SyncMaster<T> where T :  class ,new()
{
    public event EventHandler<T>? Add;
    public event EventHandler<SyncMasterArgs<T>>? Update;
    public event EventHandler<T>? Remove;

    private readonly List<T>? _source;
    private readonly List<T>? _dest;

    public SyncMaster(List<T> source, List<T> dest)
    {
        _source = source;
        _dest = dest;
    }

    public void Run()
    {
        if (_source is null || _dest is null) 
            throw new ArgumentNullException("Missing source or dest collections");
        T obj = new();
        Type  t = obj.GetType();
        PropertyInfo?  piId = t.GetProperty("Id");

        if (piId is null) throw new ArgumentException("Type missing Id property");
        
        //Add /Update destination
        foreach(T srcItem in _source??new List<T>())
        {
            T? destItem = _dest.Find(e=> {
                int srcId = (int)(piId.GetValue(e)??(object)0);
                int destId = (int)(piId.GetValue(srcItem)??(object)1);
                return srcId == destId;
            });
            //Destination doesn't have the item
            if (destItem is null)
                //Add the it
                Add?.Invoke(this, srcItem);
            else
                //Update destination with the source item
                Update?.Invoke(this, new SyncMasterArgs<T>(srcItem,destItem));
        }

        //Remove destination elements

        foreach(T destItem in _dest)
        {
            T? srcItem = _source?.Find(e=>{
                int srcId = (int)(piId.GetValue(e)??(object)0);
                int destId = (int)(piId.GetValue(destItem)??(object)1);
                return srcId == destId;
                });
            if (srcItem is null) Remove?.Invoke(this, destItem);
        }

    }


}