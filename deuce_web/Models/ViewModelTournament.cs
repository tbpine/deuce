using deuce; // Assuming the Deuce library contains these models

/// <summary>
/// ViewModel for managing tournaments, intervals, and tournament types.
/// </summary>
public class ViewModelTournament
{
    /// <summary>
    /// List of intervals (e.g., time intervals for scheduling).
    /// </summary>
    public List<Interval> Intervals { get; set; } = new();

    /// <summary>
    /// List of tournament types (e.g., single elimination, round-robin).
    /// </summary>
    public List<TournamentType> TournamentTypes { get; set; } = new();

    /// <summary>
    /// List of tournaments.
    /// </summary>
    public List<Tournament> Tournaments { get; set; } = new();

    /// <summary>
    /// Blank constructor.
    /// </summary>
    public ViewModelTournament()
    {
    }
}