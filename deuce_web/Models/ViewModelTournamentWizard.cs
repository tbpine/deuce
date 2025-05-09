using deuce;
using Microsoft.AspNetCore.Mvc.Rendering;

/// <summary>
/// Data model for the tournament wizard view model.
/// </summary>
public class ViewModelTournamentWizard
{
    protected string _userName = string.Empty;
    protected int _userId = 0;
    protected bool _loggedIn = false;
    protected string? _showBackButton;
    protected string? _backPage;
    public bool Validated { get; set; }
    public string NameValidation { get; set; } = "";

    private List<NavItem> _navItems = new();
    private List<Sport> _sports = new();
    private List<TournamentType> _tournamentTypes = new();
    private Tournament _tournament = new();
    public List<NavItem> NavItems { get => _navItems; set => _navItems = value; }
    public List<Sport> Sports { get => _sports; set => _sports = value; }
    public List<TournamentType> TournamentTypes { get => _tournamentTypes; set => _tournamentTypes = value; }
    public List<SelectListItem> Countries { get; set; } = new();

    public string? ShowBackButton { get => _showBackButton; set => _showBackButton = value; }
    public string? BackPage { get => _backPage; set => _backPage = value; }
    public Tournament Tournament { get=>_tournament; set=> _tournament = value; }
    public TournamentVenue Venue { get; set; } = new();
    public ViewModelTournamentWizard()
    { }
}