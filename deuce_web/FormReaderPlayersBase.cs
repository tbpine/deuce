
using deuce;

/// <summary>
/// Base class to implement IAdaptorTeams
/// </summary>
public abstract class FormReaderPlayersBase : IFormReaderPlayers
{
    public virtual List<Team> Parse(IFormCollection form,  Tournament tournament) =>new();
}