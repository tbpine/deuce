using MySqlX.XDevAPI;

/// <summary>
/// Account payment details page
/// </summary>
public class PaymentDetailsPageModel : BasePageModelAcc
{
    private readonly ILogger<PaymentDetailsPageModel> _log;


    public PaymentDetailsPageModel(ILogger<PaymentDetailsPageModel> log, ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config,
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