namespace deuce;

/// <summary>
/// The create factory pattern for classes
/// implementing IMatchFactory.
/// </summary>
public class FactoryCreateMatchFactory
{
    /// <summary>
    /// Create a MatchFactory from it's type.
    /// </summary>
    /// <param name="t">Tournament details</param>
    /// <returns>A type defining IMatchFactory </returns>
    public IMatchFactory Create(Tournament t)
    {
        switch (t.Type?.Id??0)
        {
            case 1: { return new MatchFactoryRR(t); }
        }

        return new MatchFactoryRR(t);
    }
}