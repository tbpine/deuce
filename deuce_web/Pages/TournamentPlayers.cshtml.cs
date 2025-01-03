using Microsoft.AspNetCore.Mvc.RazorPages;

public class TournamentPlayersPageModel : PageModel
{
    private readonly ILogger<TournamentPlayersPageModel> _log;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;

    public TournamentPlayersPageModel(ILogger<TournamentPlayersPageModel> log, IServiceProvider sp,
    IConfiguration config)
    {
        _log = log;
        _serviceProvider = sp;
        _config = config;

    }



}