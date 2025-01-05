using deuce_web.ext;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Ask to add players for the tournament or
/// wait until later
/// </summary>
public class TournamentAskPlayersPageModel : BasePageModel
{

    public string? Title { get; set; }

    [BindProperty]
    public int EntryType { get; set; }

    public string? Error { get; set; }
    
    public TournamentAskPlayersPageModel(IHandlerNavItems handlerNavItems) : base(handlerNavItems)
    {
        
    }

    public IActionResult OnGet()
    {
        this.LoadFromSession();
        Title = EntryType == 1 ? "Teams" : "Players";
        
        return Page();
    }


    public IActionResult OnPost()
    {
        this.SaveToSession();

        return Redirect(HttpContext.Request.PathBase +"/TournamentSchedule");
    }
}