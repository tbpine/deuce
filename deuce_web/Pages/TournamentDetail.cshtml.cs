using Microsoft.AspNetCore.Mvc.RazorPages;

/// <summary>
/// 
/// </summary>
public class TournamentDetailPageModel : PageModel
{
    private readonly ILogger<TournamentDetailPageModel> _log;
    public TournamentDetailPageModel(ILogger<TournamentDetailPageModel> log)
    {
        _log = log;
    }

}