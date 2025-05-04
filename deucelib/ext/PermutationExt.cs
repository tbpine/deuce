namespace deuce.ext;

/// <summary>
/// Extension methods for the "Round" class
/// </summary>
public static class PermutationEx 
{
    /// <summary>
    /// Get a string summary of the round.
    /// </summary>
    /// <param name="perm">Permutation to get the summary</param>
    /// <returns>string summary of the round.</returns>
    public static string GetSummary(this Permutation perm)
    {
        Team home = perm.GetTeamAtIndex(0);
        Team away = perm.GetTeamAtIndex(1);

        string homePlayers = String.IsNullOrEmpty(home.Label) ? home.GetPlayerCSV() : home.Label;
        string awayPlayers = String.IsNullOrEmpty(away.Label) ? away.GetPlayerCSV() : away.Label;

        if (home.Id <= 0 && away.Id > 0)
            return $"{awayPlayers} has a BYE";
        else if (away.Id <= 0 && home.Id > 0)
            return $"{homePlayers} has a BYE";
        else if (home.Id <= 0 && away.Id <= 0)
            return $"";
        else
            return $"{homePlayers} vs {awayPlayers}";
    }
}