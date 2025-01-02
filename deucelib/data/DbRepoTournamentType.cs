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

        DbDataReader reader = await cmd.ExecuteReaderAsync();
        List<TournamentType> list=new();

        while(reader.Read())
        {
            int id = reader.Parse<int>("id");
            string label = reader.Parse<string>("label");
            string name = reader.Parse<string>("name");
            string key = reader.Parse<string>("key");
            string icon = reader.Parse<string>("icon");

            list.Add(new TournamentType(id, label, name, key, icon));
        }

        reader.Close();
        
        return list;
    }
}