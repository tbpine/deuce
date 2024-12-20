using deuce.lib;

/// <summary>
/// Defines an entity that can generate matches.
/// </summary>
public interface IMatchFactory
{
    Dictionary<int, List<Match>> Produce(List<Player> players);
}