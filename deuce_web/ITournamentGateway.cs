using System.Data.Common;
using deuce;

/// <summary>
/// Denfines a source of  Tournament objects
/// </summary>
public interface ITournamentGateway
{
     Task<Tournament> GetCurrentTournament();
     Task<Tournament> GetTournament(int id);
     Task<ResultTournamentAction> StartTournament(int tournamentId);
     Task<ResultTournamentAction> ValidateCurrentTournament(Tournament tournament);
}
