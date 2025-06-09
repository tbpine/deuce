using System.Diagnostics.Contracts;
using System.Text;

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

    private List<Player> _players = new();
    private int _id;
    private int _index;
    private string _label = "";
    private Organization? _club;
    public int Id { get { return _id; } set { _id = value; } }
    public int Index { get { return _index; } set { _index = value; } }
    public string Label { get { return _label; } set { _label = value; } }
    public Organization? Club { get { return _club; } set { _club = value; } }

    public IEnumerable<Player> Players { get => _players; }

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

    /// <summary>
    /// Construct with bye players
    /// </summary>
    /// <param name="id">DB identifier</param>
    /// <param name="label">Team name</param>
    /// <param name="teamSize">Create a number players in your team</param>
    public Team(int id, string label, int noByePlayers)
    {
        _id = id;
        _label = label;
        for (int i = 0; i < noByePlayers; i++) _players.Add(new Player(true)); 
    }

    public Team()
    {

    }

    public Player GetAt(int i) => _players[i];
    public int NoPlayers { get => _players.Count; }

    /// <summary>
    /// Remove player from the team
    /// </summary>
    /// <param name="player">Player object to remove ( by id)</param>
    public void RemovePlayer(Player player)
    {
        _players.RemoveAll(e => e.Id == player.Id);
    }

    /// Remove player by team player id
    /// </summary>
    /// <param name="player">Player object to remove ( by team player id)</param>
    public void RemoveTeamPlayer(int teamPlayerId)
    {
        var playerToRemove = _players.Find(e => e.TeamPlayerId == teamPlayerId);
        if (playerToRemove is not null) _players.Remove(playerToRemove);
    }

    /// <summary>
    /// Find a player
    /// </summary>
    /// <param name="pred">Search criterion</param>
    /// <returns>Null if the search criterion failed</returns>
    public Player? Find(Predicate<Player> pred)
    {
        return _players.Find(pred);
    }
 
    ///
    ///Ranking property 
    //Get the average ranking of all players or return 0 if no players
    public double Ranking
    {
        get=> _players.Count == 0 ?   0 :  _players.Sum(p => p.Ranking) / _players.Count;
    }
}