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

        //Make a list of select items from the list of countries
        _model.Countries = (await _cache.GetList<Country>(CacheMasterDefault.KEY_ENTRY_COUNTRIES))?.Select
        (c => new SelectListItem
        {
            Value = c.Code.ToString(),
            Text = c.Name
        })?.ToList() ?? new List<SelectListItem>();
        _model.Validated = false;

        _model.Countries.Insert(0, new SelectListItem
        {
            Value = "",
            Text = ""
        });

        _model.Venue.CountryCode = 36; //Default to Australia
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
            _model.Venue.Street = loadedVenue.Street;
            _model.Venue.State = loadedVenue.State;
            _model.Venue.CountryCode = loadedVenue.CountryCode;

        }

        return View(_model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Save(ViewModelTournamentWizard viewModel)
    {
        //Tournament DTO
        viewModel.Tournament.Id = _sessionProxy?.TournamentId ?? 0 ;
        viewModel.Venue.Tournament = viewModel.Tournament;
        _dbRepoVenue.Set(viewModel.Venue);

        //Different pages for teams and individual tournaments
        int entryType = _sessionProxy?.EntryType ?? (int)deuce.EntryType.Team;

        if (entryType == (int)deuce.EntryType.Team)
            return RedirectToAction("Index", "TFormatTeam");
        else if (entryType == (int)deuce.EntryType.Individual)
            return RedirectToAction("Index", "TFormatPlayer");
            
        return View(_model);
    }
}