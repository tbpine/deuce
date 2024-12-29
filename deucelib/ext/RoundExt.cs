namespace deuce.ext;

/// <summary>
/// Extension methods for the "Round" class
/// </summary>
public static class RoundEx 
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="round">Round to count perms</param>
    /// <returns>string summary of the round.</returns>
    public static int NoGames(this Round round)
    {
        int total = 0;
        foreach(var p in round.Permutations)
            foreach(Match m in p.Matches) total++;
        return total;
    }
}