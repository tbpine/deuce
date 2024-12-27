namespace deuce.ext;

/// <summary>
/// Extension methods for the "Round" class
/// </summary>
public static class RoundEx 
{
    /// <summary>
    /// Get a string summary of the round.
    /// </summary>
    /// <param name="round">Round to get the summary</param>
    /// <returns>string summary of the round.</returns>
    public static string GetSummary(this Round round)
    {
        Team home = round.GetTeamAtIndex(0);
        Team away = round.GetTeamAtIndex(1);

        string homePlayers = String.IsNullOrEmpty(home.Label) ? home.GetPlayerCSV() : home.Label;
        string awayPlayers =  String.IsNullOrEmpty(away.Label) ? away.GetPlayerCSV() : away.Label;
        return $"{homePlayers} vs {awayPlayers}";

    }
}