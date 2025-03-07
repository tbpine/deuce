
using deuce;

/// <summary>
/// Base class to implement IAdaptorTeams
/// </summary>
public abstract class AdaptorTeamsBase : IAdaptorTeams
{
    public virtual List<Team> Convert(IFormCollection form, Organization organization) =>new();
}