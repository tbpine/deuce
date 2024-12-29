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

    private Permutation? _perm;
    private int p_id;
    
    //------------------------------------
    // Props
    //------------------------------------

    public int Round { get => _round; set => _round = value; }

    //TODO: Not all sport have this property
    public bool IsDouble{ get=> _players.Count> 2;}

    public Permutation? Permutation { get=>_perm; set=>_perm = value; }

    public IEnumerable<Player> Players { get => _players; }
    public int Id { get { return p_id; } set { p_id = value; }}
    

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

    public Match()
    {
        
    }


    public bool HasPlayer(Player val) => _players.Find(e=>e.Id == val.Id) is not null;

    public Player GetPlayerAt(int idx)=>_players[idx];

    

}