using System.Data.Common;
using System.Diagnostics;
using deuce.ext;

namespace deuce;

/// <summary>
/// Repository class for managing security-related operations.
/// </summary>
public class DbRepoSecurity : DbRepoBase<Security>
{
    /// <summary>
    /// Constructor to initialize the database connection.
    /// </summary>
    /// <param name="dbconn">Database connection object.</param>
    public DbRepoSecurity(DbConnection dbconn) : base(dbconn)
    {
    }

    /// <summary>
    /// Override the GetList method to call the stored procedure sp_check_password.
    /// </summary>
    /// <param name="filter">Filter object containing parameters for the stored procedure.</param>
    /// <returns>A list of Security objects.</returns>
    public override async Task<List<Security>> GetList(Filter filter)
    {
        List<Security> securityList = new();

        try
        {
            _dbconn.Open();

            await _dbconn.CreateReaderStoreProcAsync(
                "sp_check_password",
                new[] { "p_email", "p_password" },
                new object[] { filter.Email??"", filter.Password??"" },
                reader =>
                {
                    Security security = new()
                    {
                        Email = filter.Email,
                        Password = filter.Password,
                        IsValid = reader.Parse<bool>("IsValid")
                    };
                    securityList.Add(security);
                }
            );
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in GetList: {ex.Message}");
        }
        finally
        {
            _dbconn.Close();
        }

        return securityList;
    }
}