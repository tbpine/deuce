using Microsoft.AspNetCore.Mvc.RazorPages;

/// <summary>
/// 
/// </summary>
public class OrgPageIdxModel : PageModel
{
    private ILogger<OrgPageIdxModel> _log;

    public OrgPageIdxModel( ILogger<OrgPageIdxModel> log)
    {
        _log = log;
    }
    
}