namespace deuce;
/// <summary>
/// Results from doing an action on teams
/// </summary>
public class ResultTeamAction
{
    private string? _message;
    private RetCodeTeamAction _result;

    public string? Message { get => _message; }
    public RetCodeTeamAction? Result { get => _result; }

    /// <summary>
    /// Construct with a result and any messages
    /// </summary>
    /// <param name="result">RetCodeTeamAction enumeration</param>
    /// <param name="message">Any messages</param>
    public ResultTeamAction(RetCodeTeamAction result, string? message)
    {
        _result = result;
        _message = message;
    }

}