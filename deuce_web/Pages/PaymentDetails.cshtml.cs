/// <summary>
/// Account payment details page
/// </summary>
public class PaymentDetailsPageModel : AccBasePageModel
{
    private readonly ILogger<PaymentDetailsPageModel> _log;


    public PaymentDetailsPageModel(ILogger<PaymentDetailsPageModel> log, ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config,
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