namespace deuce;

/// <summary>
/// Defines an entity that can generate matches.
/// </summary>
public interface IScheduler
{
    Schedule Run(List<Team> team);
    //Execute every round of the schedule
    void BeforeEndRound(Schedule schedule,  int round, List<Score> scores);
    //Progress to the next round of the schedule
    void NextRound(Schedule schedule, int round, int previousRound, List<Score> scores);
}