using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using deuce.lib;
using deuce.ext;

namespace deuce;

public class DbRepoTournamentType : DbRepoBase<TournamentType>
{
    //------------------------------------
    //| Internals                         |
    //------------------------------------
    private readonly DbConnection _dbconn;

    /// <summary>
    /// Construct with dependencies
    /// </summary>
    /// <param name="dbconn">Db Connection</param>
    public DbRepoTournamentType(DbConnection dbconn)
    {
        _dbconn = dbconn;
    }

    public override async Task<List<TournamentType>> GetList()
    {
        
        DbCommand cmd = _dbconn.CreateCommand();
        cmd.CommandText = "sp_get_tournament_type";
        cmd.CommandType = CommandType.StoredProcedure;

        var reader =new SafeDataReader(await cmd.ExecuteReaderAsync());
        List<TournamentType> list=new();

        while(reader.Target.Read())
        {
            int id = reader.Target.Parse<int>("id");
            string label = reader.Target.Parse<string>("label");
            string name = reader.Target.Parse<string>("name");
            string key = reader.Target.Parse<string>("key");
            string icon = reader.Target.Parse<string>("icon");

            list.Add(new TournamentType(id, label, name, key, icon));
        }

        reader.Target.Close();
        
        return list;
    }
}