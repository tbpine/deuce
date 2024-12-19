using deuce.lib;

public class GameEngineBase
{
    protected Tournament _tournament;
    protected event EventHandler<GameCreatedEventArgs>? GameCreated;
    protected List<Player>? _players;
    protected Dictionary<int, List<Game>> _results = new();

    public GameEngineBase(Tournament tournament)
    {
        _tournament = tournament;
    }

    public Dictionary<int, List<Game>> Run(List<Player> players)
    {
        FactoryGameEngine fac = new FactoryGameEngine();
        IGameEngine? ge = fac.Create(_tournament!);
        //New tournament
        return ge?.Generate(players)!;
        
    }

    /// <summary>
    /// Game created event
    /// </summary>
    /// <param name="round">Round </param>
    /// <param name="lhs">Player 1</param>
    /// <param name="rhs">Player 2</param>
    protected void RaiseGameCreatedEvent(int round, int lhs, int rhs)
    {
        GameCreated?.Invoke(this, new GameCreatedEventArgs
        {
            Round = round,
            Lhs = lhs,
            Rhs = rhs
        });
    }

}