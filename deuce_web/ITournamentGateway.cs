using System.Data.Common;
using deuce;

/// <summary>
/// Denfines a source of  Tournament objects
/// </summary>
public interface ITournamentGateway
{
     Task<Tournament?> GetCurrentTournament();
}
