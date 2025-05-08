using Microsoft.AspNetCore.Mvc;
using deuce;
using Microsoft.AspNetCore.Mvc.Filters;

/// <summary>
/// Tournament controller
/// </summary>
public class TournamentController : Controller
{
    private string _userName = string.Empty;
    private int _userId = 0;

    private bool _loggedIn = false;
    private string? _showBackButton;
    private string? _backPage;

    private SessionProxy _sessionProxy;

    private IServiceProvider _serviceProvider;
    private IConfiguration _config;

    //Load tournament details
    private ITournamentGateway _tourGateway;

    private ISideMenuHandler? _handlerNavItems;
    public IEnumerable<NavItem>? NavItems { get => _handlerNavItems?.NavItems; }

    //This works with the _Laywizard shared page.
    //That is the button next and back buttons
    public string? ShowBackButton { get => _showBackButton; set => _showBackButton = value; }
    public string? BackPage { get => _backPage; set => _backPage = value; }

    private readonly ILogger<TournamentsPageModel> _log;
    private readonly DbRepoTournamentList _dbrepoTournamentList;
    private readonly ICacheMaster _cache;

    private List<Tournament>? _tournaments;
    //For lookups
    private List<Interval>? _intervals;
    private List<TournamentType>? _tourTypes;

    public List<Tournament>? Tournaments { get => _tournaments; }

    public List<Interval>? Intervals { get => _intervals; }
    public List<TournamentType>? TourTypes { get => _tourTypes; }

    public TournamentController(ILogger<TournamentsPageModel> log, ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config,
    ITournamentGateway tgateway, SessionProxy sessionProxy, ICacheMaster cache, DbRepoTournamentList dbrepoTournamentList)

    {
        _handlerNavItems = handlerNavItems;
        _serviceProvider = sp;
        _config = config;
        _tourGateway = tgateway;
        _sessionProxy = sessionProxy;

        _log = log;
        _cache = cache;
        _dbrepoTournamentList = dbrepoTournamentList;

    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);
        //Manage session
        if (_sessionProxy is not null) _sessionProxy.Session = context.HttpContext.Session;

        if (_handlerNavItems is null) return;

        _handlerNavItems.Set(this.HttpContext.Request.Path);

        //Find the back page
        int selectedIdx = _handlerNavItems.GetSelectedIndex();
        _showBackButton = selectedIdx > 0 ? "visible" : "invisible";
        //Set the URI of the last page in the list of
        //nav items.
        if (_showBackButton == "visible")
        {
            _backPage = HttpContext.Request.PathBase + _handlerNavItems.GetResourceAtIndex(selectedIdx - 1);
        }


    }

    public async Task<IActionResult> Index()
    {
        //get a list of tournaments
        Organization thisOrg = new Organization() { Id = 1, Name = "testing" };

        //Make a ViewModelTournament object and set properties
        ViewModelTournament vm = new();
        //Get interval for labels
        vm.Intervals = await _cache.GetList<Interval>(CacheMasterDefault.KEY_INTERVALS) ?? new();

        //Get labels for tournament type i.e Round Robbin , Knockout etc..
        vm.TournamentTypes = await _cache.GetList<TournamentType>(CacheMasterDefault.KEY_TOURNAMENT_TYPES) ?? new();

        Filter filter = new Filter() { ClubId = thisOrg.Id };

        //DTOs for touraments
        vm.Tournaments  = await _dbrepoTournamentList.GetList(filter);

        return View(vm);

    }


}