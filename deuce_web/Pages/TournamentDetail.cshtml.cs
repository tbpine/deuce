using System.Data.Common;
using System.Diagnostics;
using deuce;
using deuce_web.ext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

/// <summary>
/// 
/// </summary>
public class TournamentDetailPageModel : PageModel
{
    private readonly ILogger<TournamentDetailPageModel> _log;

    public IEnumerable<Sport>? Sports{ get; set; }
    public IEnumerable<TournamentType>? TournamentTypes{ get; set; }
    private IServiceProvider _serviceProvider;
    private IConfiguration _configuration;

    public int SelectedSportId { get; set; }
    public int SelectedTourType { get; set; }

    [BindProperty]
    public int EntryType { get; set; }

    public TournamentDetailPageModel(ILogger<TournamentDetailPageModel> log, IServiceProvider sp,
    IConfiguration config)
    {
        _log = log;
        _serviceProvider= sp;
        _configuration = config;    
    }

    public async Task<IActionResult> OnGet()
    {

        var scope = _serviceProvider.CreateScope();
        var dbconn = scope.ServiceProvider.GetService<DbConnection>();
        dbconn!.ConnectionString = _configuration.GetConnectionString("deuce_local");

        await dbconn!.OpenAsync();

        //Load page options from e from the database
        DbRepoSport dbRepoSport = new DbRepoSport(dbconn);
        Sports  = await dbRepoSport.GetList();

        DbRepoTournamentType dbRepoTourType = new DbRepoTournamentType(dbconn);
        TournamentTypes = await dbRepoTourType.GetList();

        SelectedSportId = this.HttpContext.Session.GetInt32("sport") ??1;
        //Get page values from the session
        SelectedTourType = this.HttpContext.Session.GetInt32("tournament_type")??1;

        await dbconn!.CloseAsync();

        return Page();
    }

    public IActionResult OnPost()
    {
        //Save page properties to session
        //Todo: Move manual form values

        this.SaveToSession();

        string? strSport = this.Request.Form["type"];
        string? strTournamentType = this.Request.Form["category"];
        int sportId = int.Parse(strSport ?? "");
        int tournamentType = int.Parse(strTournamentType ?? "");

        this.HttpContext.Session.SetInt32("sport", sportId);
        this.HttpContext.Session.SetInt32("tournament_type", tournamentType);
        if (EntryType == 1)
            return Redirect("/TournamentFormatTeams");
        else if (EntryType == 2)
            return Redirect("/TournamentFormatPlayers");
    }

}