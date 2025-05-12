using Microsoft.AspNetCore.Mvc.Rendering;
using deuce;

public class ViewModelFormatTeam
{
    private List<NavItem> _navItems = new();

    public string Title { get; set; } = "";
    public Format Format { get; set; } = new(1, 1, 1);

    public Tournament Tournament { get; set; } = new();
    public TournamentDetail TournamentDetail { get; set; } = new();

    public int? CustomTeamSize { get; set; }
    public int? CustomGames { get; set; }
    public int? CustomSingles { get; set; }
    public int? CustomDoubles { get; set; }
    public string? Error { get; set; }

    public List<NavItem> NavItems { get => _navItems; set => _navItems = value; }

    public IEnumerable<SelectListItem> SelectTeamSize { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> SelectGamesPerSet { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> SelectSets { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> SelectNoGames { get; set; } = new List<SelectListItem>();

    // WizardController-related properties
    public string? ShowBackButton { get; set; }
    public string? BackPage { get; set; }
}