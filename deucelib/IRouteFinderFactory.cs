namespace deuce;

/// <summary>
/// Factory interface for creating IRouteFinder implementations.
/// This interface defines the contract for creating route finder instances
/// based on tournament type or draw direction.
/// </summary>
public interface IRouteFinderFactory
{
    /// <summary>
    /// Creates a route finder based on tournament type.
    /// </summary>
    /// <param name="tournamentType">The tournament type to determine which route finder to create.</param>
    /// <returns>An IRouteFinder implementation suitable for the tournament type.</returns>
    IRouteFinder Create(int tournamentType);

    /// <summary>
    /// Creates a route finder based on tournament type details.
    /// </summary>
    /// <param name="tournamentType">The tournament type record containing detailed information.</param>
    /// <returns>An IRouteFinder implementation suitable for the tournament type.</returns>
    IRouteFinder Create(TournamentType tournamentType);
}
