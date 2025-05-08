using System.Data.Common;
using System.Diagnostics;
using deuce.ext;

namespace deuce;

/// <summary>
/// Repository class for managing Organization data.
/// </summary>
public class DbRepoOrganization : DbRepoBase<Organization>
{
    /// <summary>
    /// Constructor to initialize the database connection.
    /// </summary>
    /// <param name="dbconn">Database connection object.</param>
    public DbRepoOrganization(DbConnection dbconn) : base(dbconn)
    {
    }

    /// <summary>
    /// Override the Set method to call the stored procedure sp_set_organization.
    /// </summary>
    /// <param name="organization">The Organization object to insert or update.</param>
    public override async Task SetAsync(Organization organization)
    {
        _dbconn.Open();

        var localTran = _dbconn.BeginTransaction();
        DbCommand cmd = _dbconn.CreateCommandStoreProc(
            "sp_set_organization",
            new[] { "p_id", "p_name", "p_owner", "p_abn", "p_active" },
            new object[] { organization.Id >0 ? organization.Id : DBNull.Value, organization.Name, "", organization.Abn,
            organization.Active },
            localTran
        );

        try
        {
            object? val = await cmd.ExecuteScalarAsync();
            localTran.Commit();
            organization.Id = Convert.ToInt32(val);
        }
        catch (Exception ex)
        {
            localTran.Rollback();
            Debug.WriteLine(ex.Message);
        }
        finally
        {
            _dbconn.Close();
        }
    }
}