using deuce;



public class ViewModelMember
{
    private Account _account = new();

    private ISideMenuHandler _sideMenuHandler;

    public Account Account { get => _account; set => _account = value; }

    public IEnumerable<NavItem> NavItems { get => _sideMenuHandler.NavItems; }
    /// <summary>
    /// List of intervals (e.g., time intervals for scheduling).
    /// </summary>
    public List<Interval> Intervals { get; set; } = new();

    /// <summary>
    /// List of tournament types (e.g., single elimination, round-robin).
    /// </summary>
    public List<TournamentType> TournamentTypes { get; set; } = new();

    /// <summary>
    /// List of tournaments.
    /// </summary>
    public List<Tournament> Tournaments { get; set; } = new();
    public ViewModelMember(ISideMenuHandler sidemenu)
    {
        _sideMenuHandler = sidemenu;
    }
}