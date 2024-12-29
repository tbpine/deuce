namespace deuce;

/// <summary>
/// Get a list of games between team members
/// of a team
/// </summary>
public interface IGameMaker
{
    Permutation Create(Tournament t, Team home, Team Away, int round);
};