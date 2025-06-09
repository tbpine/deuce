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
}