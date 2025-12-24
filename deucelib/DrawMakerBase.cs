namespace deuce;

/// <summary>
/// Base class for creating tournament draws. Provides common functionality and defines the contract
/// for draw creation and handling changes during tournament progression.
/// </summary>
public class DrawMakerBase : IDrawMaker
{
    //------------------------------------
    // Internals
    //------------------------------------

    /// <summary>
    /// The tournament for which draws are being created.
    /// </summary>
    protected Tournament _tournament;

    /// <summary>
    /// List of teams participating in the tournament.
    /// </summary>
    protected List<Team> _teams = new();

    /// <summary>
    /// The size of groups used in round-robin or group-based tournament formats.
    /// </summary>
    protected int _groupSize = 4;


    /// <summary>
    /// Construct with dependencies.
    /// </summary>
    /// <param name="tournament">Tournament details</param>
    public DrawMakerBase(Tournament tournament)
    {
        _tournament = tournament;
    }

    /// <summary>
    /// Creates a draw for the tournament based on the provided teams.
    /// This method must be implemented by derived classes.
    /// </summary>
    /// <returns>A Draw object representing the tournament schedule</returns>
    public virtual Draw Create()
    {
        throw new NotImplementedException("DrawMakerBase::Create not implemented");
    }

    /// <summary>
    /// Handles changes to the draw when scores are updated, allowing for dynamic
    /// tournament progression (e.g., advancing winners, updating brackets).
    /// This method must be implemented by derived classes.
    /// </summary>
    /// <param name="schedule">The current draw schedule</param>
    /// <param name="round">The current round number</param>
    /// <param name="previousRound">The previous round number</param>
    /// <param name="scores">List of scores from recent matches</param>
    public virtual void OnChange(Draw schedule, int round, int previousRound, List<Score> scores)
    {
        throw new NotImplementedException("DrawMakerBase::OnChange not implemented");
    }

    /// <summary>
    /// Gets or sets the size of groups used in tournament formats that require grouping.
    /// Default value is 4.
    /// </summary>
    public int GroupSize { get { return _groupSize; } set { _groupSize = value; }}


}