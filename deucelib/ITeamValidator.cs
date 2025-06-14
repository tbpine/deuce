namespace  deuce;

public interface ITeamValidator
{
    /// <summary>
    /// Check a list of teams and return false if:
    /// 1. There's a player in Multiple teams
    /// 2. A player is in multiple teams
    /// </summary>
    /// <param name="teams">List of teams</param>
    /// <param name="tournament">Tournament details</param>
    /// <returns>Result of the team validation</returns>
    ResultTeamAction Check(List<Team> teams, Tournament tournament);
}