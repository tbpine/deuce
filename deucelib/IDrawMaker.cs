namespace deuce;

/// <summary>
/// Defines an entity that can generate matches.
/// </summary>
public interface IDrawMaker
{
    Draw Create(List<Team> team);
    //Execute every round of the schedule

    //Progress to the next round of the schedule
    void OnChange(Draw schedule, int round, int previousRound, List<Score> scores);
    int GroupSize { get; set; }
}