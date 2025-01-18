using System.Data;
using System.Data.Common;
using deuce.ext;
using iText.Kernel.Utils.Objectpathitems;

namespace deuce;

public class DbRepoInterval : DbRepoBase<Interval>
{
    //------------------------------------
    //| Internals                         |
    //------------------------------------
    private readonly DbConnection _dbconn;

    /// <summary>
    /// Construct with dependencies
    /// </summary>
    /// <param name="dbconn">Db Connection</param>
    public DbRepoInterval(DbConnection dbconn)
    {
        _dbconn = dbconn;
    }

    public override async Task<List<Interval>> GetList()
    {
        //Return
        List<Interval> list = new();
        //Select rows asynchronisly

        await _dbconn.CreateReaderStoreProcAsync("sp_get_interval", [], [],
        r =>
        {
            int id = r.Parse<int>("id");
            string label = r.Parse<string>("label");

            list.Add(new Interval(id, label));
        });

        return list;
    }
}