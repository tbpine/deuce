using Microsoft.AspNetCore.Mvc;
using deuce;

public class HelpCenterController : MemberController
{

    private readonly ILogger<HelpCenterController> _log;
    private readonly ICacheMaster _cache;


    public HelpCenterController(ILogger<HelpCenterController> log, ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config,
    ITournamentGateway tgateway, SessionProxy sessionProxy, ICacheMaster cache)
    :base(handlerNavItems, sp, config, tgateway, sessionProxy)
    {
        _log = log;
        _cache = cache;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(_model);
    }



}