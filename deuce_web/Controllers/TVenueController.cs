using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using deuce;

/// <summary>
/// Controller for the Tournament Wizard Venue page.
/// </summary>
public class TVenueController : WizardController
{
    private readonly ILogger<TVenueController> _log;
    private readonly DbRepoVenue _dbRepoVenue;
    private readonly ICacheMaster _cache;

    public TVenueController(ILogger<TVenueController> log, IHandlerNavItems handlerNavItems,
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
            TournamentId = _model.Tournament.Id,
            ClubId = _model.Tournament.Organization?.Id??0
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
    public IActionResult Save(ViewModelTournamentWizard src)
    {
        
        //1.Session
        //2.Form
        //Save

        _model.Venue.Tournament = _model.Tournament;
        _model.Venue.CountryCode = src.Venue.CountryCode;
        _model.Venue.Street = src.Venue.Street;
        _model.Venue.State = src.Venue.State;

        _dbRepoVenue.Set(_model.Venue);

        // Add parameters to RedirectToAction using an anonymous object
        return RedirectToAction("Index", "TFormatTeam", new {entry_type = _model.Tournament.EntryType });
    }
}