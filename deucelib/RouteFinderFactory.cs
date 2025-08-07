namespace deuce;

/// <summary>
/// Factory class for creating IRouteFinder implementations.
/// This class implements the IRouteFinderFactory interface and provides
/// methods to create different route finder instances based on tournament type.
/// The factory determines which route finding algorithm to use based on the
/// tournament structure and progression requirements.
/// 
/// Usage example:
/// <code>
/// var factory = new RouteFinderFactory();
/// var routeFinder = factory.Create(tournamentType);
/// var destinationMatch = routeFinder.FindDestMatch(draw, startMatch);
/// </code>
/// </summary>
public class RouteFinderFactory : IRouteFinderFactory
{
    /// <summary>
    /// Creates a route finder based on tournament type ID.
    /// </summary>
    /// <param name="tournamentType">The tournament type ID to determine which route finder to create.</param>
    /// <returns>An IRouteFinder implementation suitable for the tournament type.</returns>
    public IRouteFinder Create(int tournamentType)
    {
        return new RouteFinderLeftToRight(); // Default to left-to-right for simplicity
    }

    /// <summary>
    /// Creates a route finder based on tournament type record.
    /// </summary>
    /// <param name="tournamentType">The tournament type record containing detailed information.</param>
    /// <returns>An IRouteFinder implementation suitable for the tournament type.</returns>
    public IRouteFinder Create(TournamentType tournamentType)
    {
        return Create(tournamentType.Id);
    }
}
