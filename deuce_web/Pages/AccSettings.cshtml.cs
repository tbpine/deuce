using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlX.XDevAPI;

/// <summary>
/// Controller for  Account settings.
/// </summary>
public class AccSettingsPageModel : BasePageModelAcc
{
    private ILogger<AccSettingsPageModel> _log;

    /// <summary>
    /// Construct with dependencies
    /// </summary>
    /// <param name="log">Log access</param>
    /// <param name="handlerSideMenu">Side menu handler</param>
    /// <param name="sp">Service provider</param>
    /// <param name="config">Configuration framework</param>
    public AccSettingsPageModel( ILogger<AccSettingsPageModel> log,
    ISideMenuHandler handlerSideMenu, IServiceProvider sp, IConfiguration config, ITournamentGateway tgeteway, SessionProxy sessionProxy) : base(handlerSideMenu, sp, config,tgeteway, sessionProxy)
    { 
        _log = log;
    }
    
}