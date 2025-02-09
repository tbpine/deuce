using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

/// <summary>
/// The sign out page controller
/// </summary>
class SignoutPageModel : PageModel
{
    //The  log
    private readonly ILogger<SignoutPageModel> _log;
    private readonly SessionProxy _sessionProxy;

    /// <summary>
    /// Construct with dependencies
    /// </summary>
    /// <param name="log">Web log</param>
    public SignoutPageModel(ILogger<SignoutPageModel> log, SessionProxy sessionProxy)
    {
        _log = log;
        _sessionProxy = sessionProxy;
    }

    /// <summary>
    /// Clear session and return to the index index page.
    /// </summary>
    /// <returns></returns>
    public IActionResult OnGetAsync()
    {
        //Clear session 
        //Redirect back to the index page.
        //  SessionProxy sessionProxy = new SessionProxy(this.HttpContext);
         
        _sessionProxy.Clear();
        return Redirect(HttpContext.Request.PathBase + "/Index");
        
    }
}