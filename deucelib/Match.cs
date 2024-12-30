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
    private List<Player> _home = new();
    private List<Player> _away = new();
    private string? _score;
    private int _round;

    private Permutation? _perm;
    private int p_id;

    //------------------------------------
    // Props
    //------------------------------------

    public int Round { get => _round; set => _round = value; }

    //TODO: Not all sport have this property
    public bool IsDouble { get => _home.Count > 1; }

    public Permutation? Permutation { get => _perm; set => _perm = value; }

    public IEnumerable<Player> Home { get => _home; }
    public IEnumerable<Player> Away { get => _away; }
    public int Id { get { return p_id; } set { p_id = value; } }


    /// <summary>
    /// Construct with values
    /// </summary>
    /// <param name="score">Resulting statistics</param>
    /// <param name="round">If in a round robin format, the current iteration.</param>
    /// <param name="players">Who's playing in the match</param>
    public Match(string score, int round)
    {
        _score = score;
        _round = round;
    }

    public Match()
    {

    }


    public bool HasPlayer(Player val)
    {
        bool isInTeam = _home.Find(e => e.Id == val.Id) is not null;
        if (!isInTeam) isInTeam = _away.Find(e => e.Id == val.Id) is not null;
        return isInTeam;
    }
    public bool HasHomePlayer(Player val)=>_home.Contains(val);
    public bool HasAwayPlayer(Player val)=>_away.Contains(val);
    
    public Player GetHomeAt(int idx) => _home[idx];
    public Player GetAwayAt(int idx) => _away[idx];

    public void AddPairing(Player home, Player away)
    {
        if (!_home.Contains(home)) _home.Add(home);
        if (!_away.Contains(away)) _away.Add(away);
    }

    public void AddHome(Player player)
    {
        if (!_home.Contains(player)) _home.Add(player);
    }
    public void AddAway(Player player)
    {
        if (!_away.Contains(player)) _away.Add(player);
    }

}