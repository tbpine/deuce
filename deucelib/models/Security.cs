namespace deuce;

/// <summary>
/// Represents security-related data, such as user credentials and validation status.
/// </summary>
public class Security
{
    private string? _email;
    private string? _password;
    private bool _isValid;

    /// <summary>
    /// Blank constructor.
    /// </summary>
    public Security()
    {
    }

    public string? Email {get { return _email; }set { _email = value; }}
    public string? Password { get { return _password; }set { _password = value; }}

    public bool IsValid { get { return _isValid; }set { _isValid = value; }}
}