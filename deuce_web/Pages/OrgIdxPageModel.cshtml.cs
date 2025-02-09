using Microsoft.AspNetCore.Mvc.RazorPages;

/// <summary>
/// 
/// </summary>
public class OrgPageIdxModel : BasePageModelAcc
{
    private ILogger<OrgPageIdxModel> _log;

    /// <summary>
    /// Construct with dependencies
    /// </summary>
    /// <param name="log">Log access</param>
    /// <param name="handlerSideMenu">Side menu handler</param>
    /// <param name="sp">Service provider</param>
    /// <param name="config">Configuration framework</param>
    public OrgPageIdxModel( ILogger<OrgPageIdxModel> log,
    ISideMenuHandler handlerSideMenu, IServiceProvider sp, IConfiguration config, ITournamentGateway tgateway, SessionProxy sessionProxy) : base(handlerSideMenu, sp, config, tgateway,
    sessionProxy)
    { 
        _log = log;
    }
    
}