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

        //Check that all players in the teams has names
        //or are members
        bool isBlankName = teams.Any(e=> e.Players.Any(p=> (string.IsNullOrEmpty(p.First) || string.IsNullOrEmpty(p.Last)) 
        && !p.IsMember())); 

        //Missing names
        if (isBlankName) return new (RetCodeTeamAction.Error, "All players must have names.");

        //Get the list of all players
        //and check for duplicates
        List<Player> allPlayersInTeams = new();

        teams.All(e => {allPlayersInTeams.AddRange(e.Players); return true;});
        
        //LINQ
        //Non members
        var qGroupByName = from Player player in allPlayersInTeams
                            where !player.IsMember()
                            group player by (player.First , player.Last) into grp
                            select grp;

        bool hasDuplicatesNonMembers= qGroupByName.Any(e=>e.Count() > 1);

         var qGroupByNameMembers = from Player player in allPlayersInTeams
                            where player.IsMember()
                            group player by (player.Member?.Id) into grp
                            select grp;

        bool hasDuplicatesMembers= qGroupByNameMembers.Any(e=>e.Count() > 1);

        if (hasDuplicatesNonMembers || hasDuplicatesMembers ) return new (RetCodeTeamAction.Error, "A player is in multiple teams");


        //No errors found;
        return new(RetCodeTeamAction.Success, "");
    }
}