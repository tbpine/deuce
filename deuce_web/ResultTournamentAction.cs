/// <summary>
/// Records the outcome of a tournament 
/// action
/// </summary>
public class ResultTournamentAction
{
    private ResultStatus _status;
    private string _message = "";

    public ResultStatus Status { get => _status; set => _status = value; }
    public string Message { get => _message; set => _message = value; }

    public ResultTournamentAction()
    { }

    /// <summary>
    /// Construct with values.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="message"></param>
    public ResultTournamentAction(ResultStatus status, string message)
    {
        _status = status;
        _message = message;

    }

}