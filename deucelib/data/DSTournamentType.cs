using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using deuce.lib;
using deuce.ext;

namespace deuce;

class DSTournamentType : IDB<TournamentType>
{
    //------------------------------------
    //| Internals                         |
    //------------------------------------
    private readonly DbConnection _dbconn;

    /// <summary>
    /// Construct with dependencies
    /// </summary>
    /// <param name="dbconn">Db Connection</param>
    public DSTournamentType(DbConnection dbconn)
    {
        _dbconn = dbconn;
    }

    public async Task<List<TournamentType>> GetList()
    {
        //Open Connection
        await _dbconn.OpenAsync();
        
        DbCommand cmd = _dbconn.CreateCommand();
        cmd.CommandText = "sp_get_tournament_type";
        cmd.CommandType = CommandType.StoredProcedure;

        DbDataReader reader = await cmd.ExecuteReaderAsync();
        List<TournamentType> list=new();

        while(reader.Read())
        {
            int id = reader.Parse<int>("id");
            string label = reader.Parse<string>("label");

            list.Add(new TournamentType(id, label));
        }
        return list;
    }
}