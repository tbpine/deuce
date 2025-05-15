using Microsoft.AspNetCore.Mvc;
using deuce;
using Microsoft.AspNetCore.Mvc.Filters;

/// <summary>
/// Tournament controller
/// </summary>
public class TournamentController : MemberController
{

    private readonly ILogger<TournamentsPageModel> _log;
    private readonly DbRepoTournamentList _dbrepoTournamentList;
    private readonly ICacheMaster _cache;


    public TournamentController(ILogger<TournamentsPageModel> log, ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config,
    ITournamentGateway tgateway, SessionProxy sessionProxy, ICacheMaster cache, DbRepoTournamentList dbrepoTournamentList)
    :base(handlerNavItems, sp, config, tgateway, sessionProxy)
    {
        _log = log;
        _cache = cache;
        _dbrepoTournamentList = dbrepoTournamentList;
    }


    public async Task<IActionResult> Index()
    {
        //get a list of tournaments
        Organization thisOrg = new Organization() { Id = _sessionProxy.OrganizationId,
            Name = "" };

        //Make a ViewModelTournament object and set properties
        
        //Get interval for labels
        _model.Intervals = await _cache.GetList<Interval>(CacheMasterDefault.KEY_INTERVALS) ?? new();

        //Get labels for tournament type i.e Round Robbin , Knockout etc..
        _model.TournamentTypes = await _cache.GetList<TournamentType>(CacheMasterDefault.KEY_TOURNAMENT_TYPES) ?? new();

        Filter filter = new Filter() { ClubId = thisOrg.Id };

        //DTOs for touraments
        _model.Tournaments  = await _dbrepoTournamentList.GetList(filter);

        return View(_model);

    }


}