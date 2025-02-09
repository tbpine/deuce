/// <summary>
/// Account payment details page
/// </summary>
public class HelpCenterPageModel : BasePageModelAcc
{
    private readonly ILogger<HelpCenterPageModel> _log;


    public HelpCenterPageModel(ILogger<HelpCenterPageModel> log, ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config,
    ITournamentGateway tgateway, SessionProxy sessionProxy)
    :base(handlerNavItems, sp,  config, tgateway, sessionProxy)
    {
        _log = log;
    }

    public void OnGet()
    {
    }

    public void OnPost()
    {
    }


   
}