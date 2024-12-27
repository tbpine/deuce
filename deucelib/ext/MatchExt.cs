using System.Net;

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
       bool samePlayers = false;
        //If all player in the other game
        //is the same as this game.
        foreach (Player p in other.Players)
        {
            samePlayers = m.HasPlayer(p);
            if (!samePlayers) break;
        }

        return other.Players.Count() == m.Players.Count() && samePlayers &&  other.Round == m.Round;

     }

     public static string GetHomeTeam(this Match match)
     {
        return match.IsDouble ? match.GetPlayerAt(0).ToString() + "/" + match.GetPlayerAt(1).ToString() :
            match.GetPlayerAt(0).ToString();
     }

     public static string GetAwayTeam(this Match match)
     {
        return match.IsDouble ? match.GetPlayerAt(2).ToString() + "/" + match.GetPlayerAt(3).ToString() :
            match.GetPlayerAt(1).ToString();
     }

      public static string GetTitle(this Match match) => match.IsDouble ? "Doubles" : "Singles";

}