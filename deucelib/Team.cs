using System.Diagnostics.Contracts;

namespace deuce;

/// <summary>
/// Organisation of players with
/// a common goal.
/// </summary>
public class Team
{
    //------------------------------------
    //| Internals                        |
    //------------------------------------

    private List<Player> _players = new ();
    private int _id;
    private int _index;
    private string _label;
    public int Id { get { return _id; } set { _id = value; }}
    public int Index { get { return _index; } set { _index = value; }}
    public string Label { get { return _label; } set { _label = value; }}
    public IEnumerable<Player> Players {get=>_players;}
    
    public void AddPlayer(Player player)    
    {
        //List is unique
        if (!_players.Contains(player)) _players.Add(player);
    }
    
    /// <summary>
    /// Construct with dependencies
    /// </summary>
    /// <param name="id">DB identifier</param>
    /// <param name="label">Team name</param>
    public Team(int id, string label)   
    {
        _id = id;
        _label = label;
    }

    public Player GetAt(int i) => _players[i];
    public int NoPlayers { get=>_players.Count; }
    
}