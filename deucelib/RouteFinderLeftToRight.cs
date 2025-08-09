namespace deuce;

/// <summary>
/// Route finder implementation for left-to-right draw structures.
/// This class implements IRouteFinder to navigate through tournament draws
/// where the progression flows from left to right (conventional bracket format).
/// </summary>
public class RouteFinderLeftToRight : IRouteFinder
{
    /// <summary>
    /// Finds the destination match in a left-to-right draw structure.
    /// This method navigates through the draw to find the match that is the destination of the given start match.
    /// </summary>
    /// <param name="draw">The tournament draw to search within.</param>
    /// <param name="start">The starting match from which to find the destination match.</param>
    /// <param name="advanceRound">Indicates whether to advance to the next round when finding the destination match.</param>
    /// <returns>The destination Match object if found; otherwise, null.</returns>
    public Match? FindDestMatch(Draw draw, Match start, bool advanceRound = true)
    {
        // Must have a permutation to find a match
        if (start?.Permutation is null) return null;

        int nextRoundIndex = start.Round + (advanceRound ? 1 : 0);

        // Range check for next round
        if (nextRoundIndex < 1 || nextRoundIndex > draw.Rounds.Max(r => r.Index)) return null;

        // For left-to-right navigation, the calculation differs from right-to-left
        // Determine if the match are mapped up (odd positions advance differently)
        bool isOdd = (start.Permutation.Id % 2) > 0;

        // Next match index calculation for left-to-right progression
        int nextPermIdx = (start.Permutation.Id - (isOdd ? 1 : 0)) / 2;
        
        // Find the next match in the next round
        var destMatch = draw.Rounds.FirstOrDefault(r => r.Index == nextRoundIndex)?.Permutations.FirstOrDefault(p => p.Id == nextPermIdx)?.Matches.FirstOrDefault();

        return destMatch;
    }
}
