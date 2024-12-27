using Microsoft.AspNetCore.Mvc.RazorPages;

/// <summary>
/// 
/// </summary>
public class TournamentFormatPageModel : PageModel
{
    private readonly ILogger<TournamentFormatPageModel> _log;

    public TournamentFormatPageModel(ILogger<TournamentFormatPageModel> log)
    {
        _log = log;
    }
}