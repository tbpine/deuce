namespace  deuce;

public class TeamValidatorBase : ITeamValidator
{
    /// <summary>
    /// Check a list of teams and return false if:
    /// 1. There's a player in Multiple teams
    /// 2. A player is in multiple teams
    /// </summary>
    /// <param name="teams">List of teams</param>
    /// <param name="tournament">Tournament details</param>
    /// <returns>Result of the team validation</returns>
    public virtual ResultTeamAction Check(List<Team> teams, Tournament tournament)
    {
        return new ResultTeamAction(RetCodeTeamAction.Success, "Base validator does not perform any checks.");
    }
}