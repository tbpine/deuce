/// <summary>
/// Account payment details page
/// </summary>
public class HelpCenterPageModel : AccBasePageModel
{
    private readonly ILogger<HelpCenterPageModel> _log;


    public HelpCenterPageModel(ILogger<HelpCenterPageModel> log, ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config,
    ITournamentGateway tgateway)
    :base(handlerNavItems, sp,  config, tgateway)
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