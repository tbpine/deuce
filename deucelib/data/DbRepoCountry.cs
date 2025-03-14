using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using deuce.ext;

namespace deuce;

public class DbRepoCountry : DbRepoBase<Country>
{
    //------------------------------------
    //| Internals                         |
    //------------------------------------

    /// <summary>
    /// Construct with dependencies
    /// </summary>
    /// <param name="dbconn">Db Connection</param>
    public DbRepoCountry(DbConnection dbconn) : base(dbconn)
    {
    }

    public override async Task<List<Country>> GetList()
    {
        _dbconn.Open();

        List<Country> list=new();
        
        await _dbconn.CreateReaderStoreProcAsync("sp_get_country_list", [],[], reader=>{
            int code = reader.Parse<int>("country-code");
            string name = reader.Parse<string>("name");

            list.Add(new Country(code, name));

        });

        _dbconn.Close();
        
        return list;
    }
}