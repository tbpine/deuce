using Microsoft.AspNetCore.Mvc;
using deuce;

public class PaymentController : MemberController
{

    private readonly ILogger<PaymentController> _log;
    private readonly ICacheMaster _cache;


    public PaymentController(ILogger<PaymentController> log, ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config,
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