namespace deuce;

/// <summary>
/// Result of a tournament validation
/// </summary>
public class ResultTournamentValidation
{
    //------------------------------------
    // Internals
    //------------------------------------
    private string? _message;
    private bool _isValid;

    //------------------------------------
    // Props
    //------------------------------------

    //TODO: Not all sport have this property
    public bool IsValid { get => _isValid; set=>_isValid =value;}

    /// <summary>
    /// Construct with values
    /// </summary>
    /// <param name="message">Resulting statistics</param>
    /// <param name="isValid">If in a round robin format, the current iteration.</param>
    public ResultTournamentValidation(string message, bool isValid)
    {
        _message = message;
        _isValid = isValid;
    }

    public ResultTournamentValidation()
    {

    }


}