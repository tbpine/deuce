using System.Data;
using System.Data.Common;
using deuce.ext;

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
        
        DbCommand cmd = _dbconn.CreateCommand();
        cmd.CommandText = "sp_get_interval";
        cmd.CommandType = CommandType.StoredProcedure;

        DbDataReader reader = await cmd.ExecuteReaderAsync();
        List<Interval> list=new();

        while(reader.Read())
        {
            int id = reader.Parse<int>("id");
            string label = reader.Parse<string>("label");

            list.Add(new Interval(id, label));
        }

        reader.Close();
        
        return list;
    }
}