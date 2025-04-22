using System.Reflection.Metadata.Ecma335;
using Org.BouncyCastle.Crypto.Parameters;

namespace  deuce;

/// <summary>
/// Check that teams have unique players
/// </summary>
public class TeamValidator
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
    public ResultTeamAction Check(List<Team> teams)
    {
        
        if (teams.Count == 0) return new (RetCodeTeamAction.Warning, "No teams");


        //Get the list of all players
        //and check for duplicates
        List<Player> allPlayersInTeams = new();

        teams.All(e => {allPlayersInTeams.AddRange(e.Players); return true;});
        
        //LINQ
        //Non members
        var qGroupByName = from Player player in allPlayersInTeams
                            group player by (player.Id ) into grp
                            select grp;

        bool hasDuplicates= qGroupByName.Any(e=>e.Count() > 1);


        if (hasDuplicates) return new (RetCodeTeamAction.Error, "A player is in multiple teams");


        //No errors found;
        return new(RetCodeTeamAction.Success, "");
    }
}