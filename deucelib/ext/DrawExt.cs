namespace deuce.ext;

/// <summary>
/// Extension methods for the Draw class.
/// These methods provide additional functionality to the Draw class, enabling efficient searching and retrieval
/// of matches within the tournament draw structure. The Draw class represents the tournament bracket structure
/// containing rounds, permutations, and matches.
/// </summary>
public static class DrawExt
{
    /// <summary>
    /// Finds a specific match within the draw using score information.
    /// This method navigates through the draw's hierarchical structure (rounds -> permutations -> matches)
    /// to locate a match based on the provided score's round, permutation, and match identifiers.
    /// </summary>
    /// <param name="draw">The tournament draw to search within.</param>
    /// <param name="score">The score object containing round, permutation, and match identifiers.</param>
    /// <returns>The matching Match object if found; otherwise, null.</returns>
    public static Match? FindMatch(this Draw draw, Score score) =>
        draw.Rounds.FirstOrDefault(r => r.Index == score.Round)?.Permutations.FirstOrDefault(p => p.Id == score.Permutation)?.Matches.FirstOrDefault(m => m.Id == score.Match);

    /// <summary>
    /// Finds a specific match within the draw using the match ID.
    /// This method performs a comprehensive search through all rounds and permutations
    /// to locate a match with the specified ID.
    /// </summary>
    /// <param name="draw">The tournament draw to search within.</param>
    /// <param name="matchId">The unique identifier of the match to find.</param>
    /// <returns>The matching Match object if found; otherwise, null.</returns>
    public static Match? FindMatch(this Draw draw, int matchId)
    {
        foreach (var round in draw.Rounds)
        {
            foreach (var permutation in round.Permutations)
            {
                var match = permutation.Matches.FirstOrDefault(m => m.Id == matchId);
                if (match != null) return match;
            }
        }
        return null;
    }


}