using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using deuce.ext;

namespace deuce;

public class DbRepoTournamentType : DbRepoBase<TournamentType>
{
    //------------------------------------
    //| Internals                         |
    //------------------------------------

    /// <summary>
    /// Construct with dependencies
    /// </summary>
    /// <param name="dbconn">Db Connection</param>
    public DbRepoTournamentType(DbConnection dbconn) : base(dbconn)
    {
    }

    public override async Task<List<TournamentType>> GetList()
    {
        _dbconn.Open();

        List<TournamentType> list=new();
        
        await _dbconn.CreateReaderStoreProcAsync("sp_get_tournament_type", [],[], reader=>{
            int id = reader.Parse<int>("id");
            string label = reader.Parse<string>("label");
            string name = reader.Parse<string>("name");
            string key = reader.Parse<string>("key");
            string icon = reader.Parse<string>("icon");

            list.Add(new TournamentType(id, label, name, key, icon));

        });

        _dbconn.Close();
        
        return list;
    }
}