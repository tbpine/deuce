using System.Data.Common;
using deuce.ext;

namespace deuce;
public class DbRepoAccount : DbRepoBase<Account>
{


    public DbRepoAccount(DbConnection dbconn) : base(dbconn)
    {
    }

    /// <summary>
    /// Get all Accounts
    /// </summary>
    /// <param name="filter">Filter object</param>
    /// <returns></returns>
    public override async Task<List<Account>> GetList(Filter filter)
    {
        _dbconn.Open();
        //Note, player contains a member DTO.
        List<Account> accounts = new();
        await _dbconn.CreateReaderStoreProcAsync("sp_get_accounts", [], [],
        r =>
        {
            // Create a new account object and populate it with data from the reader
            Account p = new()
            {
                Id = r.Parse<int>("id"),
                Email = r.Parse<string>("email"),
                Password = r.Parse<string>("password"),
                Organization = r.Parse<int>("organization"),
                Player = r.Parse<int>("player")

            };

            accounts.Add(p);

        });

        _dbconn.Close();

        return accounts;

    }

    /// <summary>
    /// Insert or update an account
    /// </summary>
    /// <param name="obj">An account</param>
    public override void Set(Account obj)
    {
        _dbconn.Open();
        //Explicitly insert new rows if id < 1
        object primaryKeyId = obj.Id < 1 ? DBNull.Value : obj.Id;

        var command = _dbconn.CreateCommandStoreProc("sp_set_account", ["p_id",  "p_email", "p_password",
        "p_type", "p_player", "p_organization", "p_active", "p_country"], [primaryKeyId, obj.Email, obj.Password, obj.Type, obj.Player, obj.Organization, obj.Active,
        obj.Country]);

        obj.Id = command.GetIntegerFromScaler(command.ExecuteScalar());

        _dbconn.Close();

    }
}