namespace deuce;

/// <summary>
/// Defines an entity that can generate matches.
/// </summary>
public interface IDrawMaker
{
    Draw Create();
    //Execute every round of the schedule

    //Progress to the next round of the schedule
    void OnChange(Draw schedule, int round, int previousRound, List<Score> scores);
    int GroupSize { get; set; }

    //See if the draw has changed
    bool HasChanged(Draw? schedule, int round, int previousRound, List<Score> scores);
    
}