using System.Data.Common;
using deuce;
using deuce_web.ext;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// 
/// </summary>
public class TournamentDetailPageModel : BasePageModel
{
    private readonly ILogger<TournamentDetailPageModel> _log;

    public IEnumerable<Sport>? Sports { get; set; }
    public IEnumerable<TournamentType>? TournamentTypes { get; set; }
    private IServiceProvider _serviceProvider;
    private IConfiguration _configuration;

    [BindProperty]
    public int SelectedSportId { get; set; }

    [BindProperty]
    public int SelectedTourType { get; set; }

    [BindProperty]
    public int EntryType { get; set; }

    [BindProperty]
    public string EventLabel { get; set; } = "";


    public TournamentDetailPageModel(ILogger<TournamentDetailPageModel> log, IServiceProvider sp,
    IConfiguration config, IHandlerNavItems hNavItems) : base(hNavItems)
    {
        _log = log;
        _serviceProvider = sp;
        _configuration = config;
    }

    public async Task<IActionResult> OnGet()
    {

        try
        {
            await LoadPage();
        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message);
        }


        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        //Save page properties to session
        //Todo: Move manual form values
        string tmpEventLabel = string.IsNullOrEmpty(EventLabel) ? Randomizer.GetRandomString(32) : EventLabel;
        EventLabel = tmpEventLabel;

        this.SaveToSession();

        using (var scope = _serviceProvider.CreateScope())
        {
            DbConnection? dbconn = scope.ServiceProvider.GetService<DbConnection>();
            if (dbconn is not null)
            {
                dbconn.ConnectionString = _configuration.GetConnectionString("deuce_local");
                await dbconn?.OpenAsync()!;

                Tournament tournament = new();

                //Load the current tournament id
                int currentTournamentId = this.HttpContext.Session.GetInt32("CurrentTournament") ?? 0;
                Organization org = new Organization() { Id = 1, Name = "testing" };
                tournament.Id = currentTournamentId;
                tournament.Label = EventLabel;
                tournament.Sport = SelectedSportId;
                tournament.Type = SelectedTourType;
                tournament.Organization = org;
                tournament.EntryType = EntryType;

                DbRepoTournament dbrepoTour = new DbRepoTournament(dbconn, org);
                //Save the tournament to db
                await dbrepoTour.Set(tournament);
                //Save tournament id

                this.HttpContext.Session.SetInt32("CurrentTournament", tournament.Id);

            }
            await dbconn?.CloseAsync()!;

        }

        if (EntryType == 1)
            return NextPage("/TournamentFormatTeams");
        else if (EntryType == 2)
            return NextPage("/TournamentFormatPlayer");

        return Page();
    }

    private async Task LoadPage()
    {
        var scope = _serviceProvider.CreateScope();

        var dbconn = scope.ServiceProvider.GetService<DbConnection>();
        dbconn!.ConnectionString = _configuration.GetConnectionString("deuce_local");
        await dbconn!.OpenAsync();

        //Load page options from the from the database
        DbRepoSport dbRepoSport = new DbRepoSport(dbconn);
        Sports = await dbRepoSport.GetList();

        DbRepoTournamentType dbRepoTourType = new DbRepoTournamentType(dbconn);

        TournamentTypes = await dbRepoTourType.GetList();

        await dbconn!.CloseAsync();
        // this.LoadFromSession();
        //Load from database
        //Set default vales;

        Organization organization = new Organization() { Id = 1, Name = "testing" };
        
        Tournament? currentTour = await this.GetCurrentTournament(_serviceProvider, _configuration, organization);
        if (currentTour is not null)
        {
            SelectedSportId = currentTour.Sport;
            SelectedTourType = currentTour.Type;
            EventLabel = currentTour.Label ?? EventLabel;
            EntryType = currentTour.EntryType;
        }
        else
        {
            SelectedSportId = 1;
            SelectedTourType = 1;
            EventLabel = "";
            EntryType = 1;
            //Default values
        }

    }

}