using Microsoft.AspNetCore.Mvc;
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

    public string? Title { get; set; }

    public IActionResult OnGet()
    {
        int sportId = this.HttpContext.Session.GetInt32("sport")??0;
        int tournamentType = this.HttpContext.Session.GetInt32("tournament_type")??0;

        

        return Page();
        
    }
}