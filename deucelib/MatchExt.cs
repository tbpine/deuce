namespace deuce.lib.ext;


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

}