using System.Reflection.Metadata.Ecma335;

namespace deuce;

/// <summary>
/// A competitive activity between players.
/// </summary>
public class Match
{
    //------------------------------------
    // Internals
    //------------------------------------
    private List<Player> _players = new();
    private string? _score;
    private int _round;

    //------------------------------------
    // Props
    //------------------------------------

    public int Round { get => _round; set => _round = value; }

    public IEnumerable<Player> Players { get => _players; }

    /// <summary>
    /// Construct with values
    /// </summary>
    /// <param name="score">Resulting statistics</param>
    /// <param name="round">If in a round robin format, the current iteration.</param>
    /// <param name="players">Who's playing in the match</param>
    public Match(string score, int round, params Player[] players)
    {
        _score = score;
        _players.AddRange(players);
        _round = round;
    }


    public bool HasPlayer(Player val) => _players.Find(e=>e.Id == val.Id) is not null;

}