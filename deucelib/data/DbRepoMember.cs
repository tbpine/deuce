using System.Data.Common;
using System.Net;
using deuce.ext;

namespace deuce;
public class DbRepoMember : DbRepoBase<Member>
{
    

    public DbRepoMember(DbConnection dbconn) : base(dbconn)
    {
    }
    
    /// <summary>
    /// Get a list of member from a country
    /// </summary>
    /// <param name="filter">Filter object</param>
    /// <returns></returns>
    public override async Task<List<Member>> GetList(Filter filter)
    {
        _dbconn.Open();

        List<Member> members = new();
        await _dbconn.CreateReaderStoreProcAsync("sp_get_member_list", ["p_country_code" ], [ filter.CountryCode ],
        r =>
        {
            Member m = new()
            {
                Id = r.Parse<int>("id"),
                First = r.Parse<string>("first_name"),
                Last = r.Parse<string>("last_name"),
                Middle = r.Parse<string>("middle_name"),
                Utr = r.Parse<double>("utr"),
                CountryCode = r.Parse<int>("country-code")
            };

            members.Add(m);

        });

        _dbconn.Close();

        return members;

    }

    public override void Set(Member obj)
    {
        _dbconn.Open();
         //Explicitly insert new rows if id < 1
        object primaryKeyId = obj.Id < 1 ? DBNull.Value : obj.Id;

        var command = _dbconn.CreateCommandStoreProc("sp_set_member", ["p_id",  "p_first_name", "p_last_name",
        "p_middle_name", "p_utr", "p_country_code"], [primaryKeyId, obj.First, obj.Last , obj.Middle,   obj.Utr,
        obj.CountryCode ]);
        obj.Id = command.GetIntegerFromScaler(command.ExecuteScalar());

        _dbconn.Close();

    }
}