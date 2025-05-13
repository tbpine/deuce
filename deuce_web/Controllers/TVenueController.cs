using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using deuce;

/// <summary>
/// Controller for the Tournament Wizard Venue page.
/// </summary>
public class TVenueController : WizardController
{
    private readonly ILogger<TournamentVenuePageModel> _log;
    private readonly DbRepoVenue _dbRepoVenue;
    private readonly ICacheMaster _cache;

    public TVenueController(ILogger<TournamentVenuePageModel> log, IHandlerNavItems handlerNavItems,
   IConfiguration cfg, IServiceProvider sp, DbRepoVenue dbrepoVenue, ICacheMaster cache) : base(handlerNavItems, sp, cfg)
    {
        _log = log;
        _dbRepoVenue = dbrepoVenue;
        _cache = cache;

    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {

        ViewModelTournamentWizard viewModel = new ViewModelTournamentWizard();
        //Set back page properties
        viewModel.ShowBackButton = _showBackButton;
        viewModel.BackPage = _backPage;
        
        //Make a list of select items from the list of countries
        viewModel.Countries = (await _cache.GetList<Country>(CacheMasterDefault.KEY_ENTRY_COUNTRIES))?.Select
        (c => new SelectListItem
        {
            Value = c.Code.ToString(),
            Text = c.Name
        })?.ToList() ?? new List<SelectListItem>();
        viewModel.Validated = false;

        viewModel.Countries.Insert(0, new SelectListItem
        {
            Value = "",
            Text = ""
        });

        viewModel.Venue.CountryCode = 36; //Default to Australia
        //Load the venue where this tournament is
        //to be held.

        Filter filter = new Filter()
        {
            TournamentId = _sessionProxy.TournamentId,
            ClubId = _sessionProxy.OrganizationId
        };

        var listOfVenues = await _dbRepoVenue.GetList(filter);

        //Set values
        if (listOfVenues.Count > 0)
        {
            TournamentVenue loadedVenue = listOfVenues[0];
            //Set binded properties that will
            //be displayed on the page.
            viewModel.Venue.Street = loadedVenue.Street;
            viewModel.Venue.State = loadedVenue.State;
            viewModel.Venue.CountryCode = loadedVenue.CountryCode;

        }
        //Menus
        viewModel.NavItems = new List<NavItem>(_handlerNavItems?.NavItems ?? Enumerable.Empty<NavItem>());

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Save(ViewModelTournamentWizard viewModel)
    {
        //Tournament DTO
        Tournament tourDTO = new Tournament() { Id = _sessionProxy?.TournamentId ?? 0 };
        viewModel.Venue.Tournament = tourDTO;
        _dbRepoVenue.Set(viewModel.Venue);

        //Different pages for teams and individual tournaments
        int entryType = _sessionProxy?.EntryType ?? (int)deuce.EntryType.Team;

        if (entryType == (int)deuce.EntryType.Team)
            return RedirectToAction("Index", "TFormatTeam");
        else if (entryType == (int)deuce.EntryType.Individual)
            return RedirectToAction("Index", "TFormatPlayer");
            
        return View(viewModel);
    }
}