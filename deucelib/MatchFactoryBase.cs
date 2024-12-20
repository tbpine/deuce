using deuce.lib;

/// <summary>
/// Common attributes amongst classes that produce matches.
/// </summary>
public class MatchFactoryBase
{
    //------------------------------------
    // Internals
    //------------------------------------
    protected Tournament _tournament;
    protected event EventHandler<MatchCreatedEventArgs>? GameCreated;
    protected List<Player> _players = new();
    protected Dictionary<int, List<Match>> _results = new();

    /// <summary>
    /// Constuct with dependencies.
    /// </summary>
    /// <param name="tournament">Tournament details</param>
    public MatchFactoryBase(Tournament tournament)
    {
        _tournament = tournament;
    }

    /// <summary>
    /// Create a match factory and make matches.
    /// </summary>
    /// <param name="players">Players in a tournament</param>
    /// <param name="tournament">Tournament details</param>
    /// <returns></returns>
    public static Dictionary<int, List<Match>> Run(List<Player> players, Tournament tournament)
    {
        FactoryCreateMatchFactory fac = new FactoryCreateMatchFactory();
        IMatchFactory ge = fac.Create(tournament);
        //New tournament
        return ge.Produce(players);

    }

    /// <summary>
    /// Game created event
    /// </summary>
    /// <param name="round">Round </param>
    /// <param name="lhs">Player 1</param>
    /// <param name="rhs">Player 2</param>
    protected void RaiseGameCreatedEvent(int round, int lhs, int rhs)
    {
        GameCreated?.Invoke(this, new MatchCreatedEventArgs
        {
            Round = round,
            Lhs = lhs,
            Rhs = rhs
        });
    }

}