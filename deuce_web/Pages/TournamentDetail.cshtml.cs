using System.Data.Common;
using System.Diagnostics;
using deuce;
using deuce_web.ext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

/// <summary>
/// 
/// </summary>
public class TournamentDetailPageModel : BasePageModel
{
    private readonly ILogger<TournamentDetailPageModel> _log;

    public IEnumerable<Sport>? Sports{ get; set; }
    public IEnumerable<TournamentType>? TournamentTypes{ get; set; }
    private IServiceProvider _serviceProvider;
    private IConfiguration _configuration;

    [BindProperty]
    public int SelectedSportId { get; set; }
    
    [BindProperty]
    public int SelectedTourType { get; set; }

    [BindProperty]
    public int EntryType { get; set; }


    public TournamentDetailPageModel(ILogger<TournamentDetailPageModel> log, IServiceProvider sp,
    IConfiguration config, IHandlerNavItems hNavItems) : base(hNavItems)
    {
        _log = log;
        _serviceProvider= sp;
        _configuration = config;
    }

    public async Task<IActionResult> OnGet()
    {

        this.LoadFromSession();

        var scope = _serviceProvider.CreateScope();

        var dbconn = scope.ServiceProvider.GetService<DbConnection>();
        dbconn!.ConnectionString = _configuration.GetConnectionString("deuce_local");

        await dbconn!.OpenAsync();

        //Load page options from e from the database
        DbRepoSport dbRepoSport = new DbRepoSport(dbconn);
        Sports  = await dbRepoSport.GetList();

        DbRepoTournamentType dbRepoTourType = new DbRepoTournamentType(dbconn);
        TournamentTypes = await dbRepoTourType.GetList();


        await dbconn!.CloseAsync();
        if (SelectedSportId == 0) SelectedSportId = 1;
        if (SelectedTourType == 0) SelectedTourType = 1;
        if (EntryType == 0) EntryType = 1;

        

        return Page();
    }

    public IActionResult OnPost()
    {
        //Save page properties to session
        //Todo: Move manual form values

        this.SaveToSession();

        if (EntryType == 1)
            return NextPage("/TournamentFormatTeams");
        else if (EntryType == 2)
            return NextPage("/TournamentFormatPlayer");

        return Page();
    }

}