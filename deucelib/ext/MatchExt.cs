using System.Net;
using System.Text;

namespace deuce.ext;


/// <summary>
/// Match class extensions.
/// </summary>
public static class MatchExt {
    
    /// <summary>
    /// True if two matches have the same players in a 
    /// the same round.
    /// </summary>
    /// <param name="m">Lhs</param>
    /// <param name="other">Rhs</param>
    /// <returns>True if two matches have the same players in a the same round.</returns>
    public static bool IsSameGame (this Match m, Match other)
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
               isSameHomePlayers &&  isSameAwayPlayers &&  other.Round == m.Round;

     }

     public static string GetHomeTeam(this Match match)
     {
        StringBuilder sb= new();
        for (int i = 0; i < match.Home.Count(); i++)
            sb.Append(match.GetHomeAt(i).ToString() + (i == match.Home.Count()-1 ? "" : "/"));
        return sb.ToString();
     }

     public static string GetAwayTeam(this Match match)
     {
        StringBuilder sb= new();
        for (int i = 0; i < match.Away.Count(); i++)
            sb.Append(match.GetAwayAt(i).ToString() + (i == match.Away.Count()-1 ? "" : "/"));
        return sb.ToString();
     }

      public static string GetTitle(this Match match) => match.IsDouble ? "Doubles" : "Singles";

}