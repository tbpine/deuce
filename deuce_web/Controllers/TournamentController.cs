using Microsoft.AspNetCore.Mvc;
using deuce;
using Microsoft.AspNetCore.Mvc.Filters;

/// <summary>
/// Tournament controller
/// </summary>
public class TournamentController : Controller
{
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

    }

    public async Task<IActionResult> Index()
    {
        //get a list of tournaments
        Organization thisOrg = new Organization() { Id = _sessionProxy.OrganizationId,
            Name = "" };

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