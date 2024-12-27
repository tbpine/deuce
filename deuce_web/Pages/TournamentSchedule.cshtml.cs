using Microsoft.AspNetCore.Mvc.RazorPages;

/// <summary>
/// 
/// </summary>
public class TournamentSchedulePageModel : PageModel
{
    private readonly ILogger<TournamentSchedulePageModel> _log;

    public TournamentSchedulePageModel(ILogger<TournamentSchedulePageModel> log)
    {
        _log = log;
    }
}