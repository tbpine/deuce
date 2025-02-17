using Org.BouncyCastle.Asn1.Crmf;

namespace deuce;
/// <summary>
/// Outcomes of a tournament scheduling.
/// </summary>
public sealed class ResultSchedule
{

    private RetCodeScheduling _retCode;
    private string? _lastError;

    public RetCodeScheduling RetCode { get => _retCode; }

    public string? LastError { get => _lastError; }

    /// <summary>
    /// Create the result of a tournament scheduling
    /// </summary>
    /// <param name="retCode">Error code if any</param>
    /// <param name="lastError">Error Message</param>
    public ResultSchedule(RetCodeScheduling retCode, string? lastError)
    {
        _retCode = retCode;
        _lastError = lastError;
    }

}