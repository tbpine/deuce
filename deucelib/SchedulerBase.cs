namespace deuce;

/// <summary>
/// Common attributes amongst classes that produce matches.
/// </summary>
public class SchedulerBase
{
    //------------------------------------
    // Internals
    //------------------------------------
    protected Tournament _tournament;
    protected List<Team> _teams = new();
   

    /// <summary>
    /// Constuct with dependencies.
    /// </summary>
    /// <param name="tournament">Tournament details</param>
    public SchedulerBase(Tournament tournament)
    {
        _tournament = tournament;
    }

    

}