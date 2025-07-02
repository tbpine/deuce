namespace deuce.ext;

/// <summary>
/// Extension methods for the Schedule class.
/// These methods provide additional functionality to the Schedule class, such as finding matches based on scores.
/// This class is designed to work with the Schedule class, which defines when, where, and who should play matches.
/// </summary>
public static class ScheduleExt
{
    /// <summary>
    /// Find a match in the schedule based on a score.
    /// This method searches through the rounds, permutations, and matches to find the match that corresponds
    /// </summary>
    /// <param name="schedule"> The schedule to search within.</param>
    /// <param name="score"> The score containing the round, permutation, and match identifiers.</param>
    /// <returns></returns>
    public static Match? FindMatch(this Schedule schedule, Score score) =>
        //Get the score's round, permutation, and match
        schedule.Rounds.FirstOrDefault(r => r.Index == score.Round)?.Permutations.FirstOrDefault(p => p.Id == score.Permutation)?.Matches.FirstOrDefault(m => m.Id == score.Match);
    //If the round is not null, return the match

}