namespace deuce;

/// <summary>
/// Defines an entity that can generate matches.
/// </summary>
public interface IScheduler
{
    Schedule Run(List<Team> team);
}