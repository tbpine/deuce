namespace deuce;

/// <summary>
/// Interface for security-related operations.
/// </summary>
public interface ISecurityGateway
{
    /// <summary>
    /// Checks the password for the given email and returns account details if successful.
    /// </summary>
    /// <param name="email">The email address to authenticate.</param>
    /// <param name="password">The password to authenticate.</param>
    /// <returns>An Account object if authentication is successful; otherwise, null.</returns>
    Task<Account?> CheckPasswordAsync(string email, string password);
}