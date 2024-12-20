namespace deuce.lib;

/// <summary>
/// Supply game details to the event handler.
/// </summary>
public class MatchCreatedEventArgs
{
    public int Round { get; set; }
    public int Lhs { get; set; }
    public int Rhs { get; set; }

}