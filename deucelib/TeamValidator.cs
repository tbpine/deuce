namespace  deuce;

/// <summary>
/// Check that teams have unique players . For entry type "team"
/// </summary>
public class TeamValidator : TeamValidatorBase
{

    public TeamValidator()
    {

    }

    /// <summary>
    /// Check a list of teams and return false if:
    /// 1. There's a player in Multiple teams
    /// 2. A player is in multiple teams
    /// </summary>
    /// <param name="teams">List of teams</param>
    /// <returns>false if invalid teams</returns>
    public override ResultTeamAction Check(List<Team> teams, Tournament tournament)
    {
        
        if (teams.Count == 0) return new(RetCodeTeamAction.Warning, "No teams");


        //Get the list of all players
        //and check for duplicates
        List<Player> allPlayers = new();

        teams.All(e => {allPlayers.AddRange(e.Players); return true;});

        //Check that teams have the specified number of players
        foreach (var team in teams)
        {
            if (team.Players.Count() != tournament.Details.TeamSize)
                return new(RetCodeTeamAction.Error, $"Invalid number of players in team {team.Label}");
        }
        
        //No errors found;
        return new(RetCodeTeamAction.Success, "");
    }
}