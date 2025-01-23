using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

/// <summary>
/// The sign out page controller
/// </summary>
class SignoutPageModel : PageModel
{
    //The  log
    private readonly ILogger<SignoutPageModel> _log;

    /// <summary>
    /// Construct with dependencies
    /// </summary>
    /// <param name="log">Web log</param>
    public SignoutPageModel(ILogger<SignoutPageModel> log)
    {
        _log = log;
    }

    /// <summary>
    /// Clear session and return to the index index page.
    /// </summary>
    /// <returns></returns>
    public IActionResult OnGetAsync()
    {
        //Clear session 
        //Redirect back to the index page.
        SessionProxy sessionProxy = new SessionProxy(this.HttpContext.Session);
        sessionProxy.Clear();
        return Redirect(HttpContext.Request.PathBase + "/Index");
        
    }
}