using System.Net;
using System.Text;

namespace deuce.ext;


/// <summary>
/// Match class extensions.
/// </summary>
public static class MatchExt
{

    /// <summary>
    /// True if two matches have the same players in a 
    /// the same round.
    /// </summary>
    /// <param name="m">Lhs</param>
    /// <param name="other">Rhs</param>
    /// <returns>True if two matches have the same players in a the same round.</returns>
    public static bool IsSameGame(this Match m, Match other)
    {
        //If all player in the other game
        //is the same as this game.
        bool isSameHomePlayers = true;
        bool isSameAwayPlayers = true;

        //Check home players
        foreach (Player p in other.Home)
        {
            isSameHomePlayers = m.HasHomePlayer(p);
            if (!isSameHomePlayers) return false;
        }

        //Check away players
        foreach (Player p in other.Away)
        {
            isSameAwayPlayers = m.HasAwayPlayer(p);
            if (!isSameAwayPlayers) return false;
        }


        return other.Home.Count() == m.Home.Count() &&
               other.Away.Count() == m.Away.Count() &&
               isSameHomePlayers && isSameAwayPlayers && other.Round == m.Round;

    }

    public static string GetHomeTeam(this Match match)
    {

        StringBuilder sb = new();
        for (int i = 0; i < match.Home.Count(); i++)
        {
            //if the player is a bye player then make sb = "BYE", then break out of the loop
            if (match.GetHomeAt(i).Bye)
            {
                sb.Clear();
                sb.Append("BYE");
                break;
            }

            sb.Append(match.GetHomeAt(i).ToString() + (i == match.Home.Count() - 1 ? "" : "/"));
        }
        return sb.ToString();
    }

    public static string GetAwayTeam(this Match match)
    {

        StringBuilder sb = new();
        for (int i = 0; i < match.Away.Count(); i++)
        {
            //if the player is a bye player then make sb = "BYE", then break out of the loop
            if (match.GetAwayAt(i).Bye)
            {
                sb.Clear();
                sb.Append("BYE");
                break;
            }

            sb.Append(match.GetAwayAt(i).ToString() + (i == match.Away.Count() - 1 ? "" : "/"));
        }
        return sb.ToString();
    }

    public static string GetTitle(this Match match) => match.IsDouble ? "Doubles" : "Singles";

    //returns true if this match is a bye match
    public static bool IsByeMatch(this Match match)
    {
        //Return true if either home or away team id is less than 0
        //or if the home team is empty and the away team is empty.
        return (match.Home.FirstOrDefault()?.Bye ?? false) || (match.Away.FirstOrDefault()?.Bye ?? false);

    }

    /// <summary>
    /// Returns the losing side of a match given the winning team.
    /// If the winning team is not found in the match, returns null.    
    /// </summary>
    /// <param name="match">Match object containing the teams</param>
    /// <param name="winner">The winning team</param>
    /// <returns>The losing team, or null if not found</returns>
    public static Team? GetLosingSide(this Match match, Team? winner)
    {
        if (match.Home.FirstOrDefault()?.Team == winner)
        {
            return match.Away.FirstOrDefault()?.Team;
        }
        else if (match.Away.FirstOrDefault()?.Team == winner)
        {
            return match.Home.FirstOrDefault()?.Team;
        }

        return null;
    }
}