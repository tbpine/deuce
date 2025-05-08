using System.Data.Common;

namespace deuce;

/// <summary>
/// Default implementation of the ISecurityGateway interface for security-related operations.
/// </summary>
public class SecurityGatewayDefault : ISecurityGateway
{
    private readonly DbConnection _dbConnection;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityGatewayDefault"/> class.
    /// /// </summary>
    /// <param name="dbConnection">The database connection to use.</param>
    /// <remarks>
    public SecurityGatewayDefault(DbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<Account?> CheckPasswordAsync(string email, string password)
    {
        //Get a list of security objects with this email and 
        //password
        //Check if the password is valid

        //Make the filter
        //Connect to the database and get the list of securities
        Filter filter = new() { Email = email, Password = password };
        DbRepoSecurity dbrepoSec = new(_dbConnection);
        DbRepoAccount dbRepoAcc = new(_dbConnection);

        var listOfSecurities = await dbrepoSec.GetList(filter);
        bool passed = (listOfSecurities?.Count ?? 0) > 0 && (listOfSecurities?.First().IsValid ?? false);

        if (passed)
            return (await dbRepoAcc.GetList(filter)).FirstOrDefault();

        return null;

    }
 
} 
