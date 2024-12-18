using System.ComponentModel;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace deuce.lib;
public class Game
{

    private List<Player> _players = new();
    private string? _score;
    private int _round;

    public int Round { get => _round; set=>_round = value; }
    
    public Game(string score, int round, params Player[] player)
    {
        _score = score;
        _players.AddRange(player);  
        _round = round;
    }
    public IEnumerable<Player> Players { get=>_players; }

    public bool IsSameGame(Game other)
    {
        bool samePlayers = false;
        //If all player in the other game
        //is the same as this game.
        foreach (Player p in other.Players) 
        {
            samePlayers = _players.Find(e=>e.Id == p.Id) != null;
            if (!samePlayers) break;
        }

        return other.Players.Count() == _players.Count && samePlayers && _round == other.Round;

    }

  
}