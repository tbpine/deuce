namespace deuce;

/// <summary>
/// The create factory pattern for classes
/// implementing IMatchFactory.
/// </summary>
public class FactorySchedulers
{
    /// <summary>
    /// Create a MatchFactory from it's type.
    /// </summary>
    /// <param name="t">Tournament details</param>
    /// <returns>A type defining IMatchFactory </returns>
    public IScheduler Create(Tournament t, IGameMaker gm)
    {
        switch (t.Type)
        {
            case 2: { return new SchedulerKnockOut(t, gm); }
        }

        return new SchedulerRR(t, gm);
    }

    /// <summary>
    /// Create a scheduler for different tournament types.
    /// </summary>
    /// <param name="tType"> Tournament type</param>
    /// <param name="t">Tournament details</param>
    /// <param name="gm">Game maker</param>
    /// <returns>A scheduler for the specified tournament type</returns>
    public IScheduler Create(TournamentType tType, Tournament t, IGameMaker gm)
    {
        switch (tType.Id)
        {
            case 3: { return new SchedulerBrackets(t, gm); }
            case 2: { return new SchedulerKnockOut(t, gm); }
            default: { return new SchedulerRR(t, gm); }
        }

    }

}