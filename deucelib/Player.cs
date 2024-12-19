namespace deuce.lib;
public class Player
{
    private List<Game> _games = new();
    private int _id;
    private string? _first;
    private double _ranking;
    private string? _last;
    private int _index;

    public int Id { get { return _id; } set { _id = value; } }
    public string? First { get { return _first; } set { _first = value; } }
    public string? Last { get { return _last; } set { _last = value; } }
    public double Ranking { get { return _ranking; } set { _ranking = value; } }


    public int Index { get { return _index; } set { _index = value; } }
    public Player()
    {

    }

    public static List<Player> ExcList(Player player, List<Player> pool)
    {
        //Get everone involved in this player's games.
        List<Player> tmp = new();
        foreach (var g in player._games)
        {
            foreach (var p in g.Players)
                if (!tmp.Contains(p)) tmp.Add(p);
        }

        return pool.FindAll(x => !tmp.Contains(x) && x.Id != player.Id);


    }
    public void AddGame(Game game) => _games.Add(game);

    public IEnumerable<Game> Games { get => _games; }

}