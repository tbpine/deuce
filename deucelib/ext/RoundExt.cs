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
        foreach (var p in round.Permutations)
            foreach (Match m in p.Matches) total++;
        return total;
    }

    /// <summary>
    /// Find a match in the round by home and away team IDs.
    /// </summary>
    /// <param name="homeId">Home team ID</param>
    /// <param name="awayId">Away team ID</param>
    /// <returns>A tuple containing the found match and the round it belongs to, or null if not found.</returns>
    public static (Match?, Round?) FindMatch(this Round round, int homeId, int awayId)
    {
        //check the main round
        foreach (var p in round.Permutations)
        {
            foreach (Match m in p.Matches)
            {
                if (m.Home.Any(e => e.Id == homeId) && m.Away.Any(e => e.Id == awayId))
                    return (m, round);
            }
        }

        //If the match is not in the main round, check the playoff round

        if (round.Playoff is not null)
        {
            foreach (var p in round.Playoff.Permutations)
            {
                foreach (Match m in p.Matches)
                {
                    if (m.Home.Any(e => e.Id == homeId) && m.Away.Any(e => e.Id == awayId))
                        return (m, round.Playoff);
                }
            }

        }

        return (null, null); // Not found
    }

}
