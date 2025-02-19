using System.Data.Common;
using deuce;
using deuce_web.ext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

/// <summary>
/// 
/// </summary>
public class TournamentDetailPageModel : BasePageModelWizard
{
    private readonly ILogger<TournamentDetailPageModel> _log;

    public IEnumerable<Sport>? Sports { get; set; }
    public IEnumerable<TournamentType>? TournamentTypes { get; set; }

    [BindProperty]
    public int SelectedSportId { get; set; }

    [BindProperty]
    public int SelectedTourType { get; set; }

    [BindProperty]
    public int EntryType { get; set; }

    [BindProperty]
    public string EventLabel { get; set; } = "";


    public TournamentDetailPageModel(ILogger<TournamentDetailPageModel> log, IServiceProvider sp,
    IConfiguration config, IHandlerNavItems hNavItems) : base(hNavItems, sp, config)
    {
        _log = log;
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

        using (var scope = _serviceProvider.CreateScope())
        {
            DbConnection? dbconn = scope.ServiceProvider.GetService<DbConnection>();
            if (dbconn is not null)
            {
                dbconn.ConnectionString = _config.GetConnectionString("deuce_local");
                await dbconn?.OpenAsync()!;

                //Load the current tournament
                //.Set the status for new tournament
                Tournament? tournament = (_sessionProxy?.TournamentId ?? 0) > 0 ?
                 await GetCurrentTournament(dbconn) : null;
                if (tournament is null) tournament = new();
                
                //Load the current tournament id
                int currentTournamentId = _sessionProxy?.TournamentId??0;
                Organization org = new Organization() { Id = 1, Name = "testing" };

                tournament.Id = currentTournamentId;
                tournament.Label = EventLabel;
                tournament.Sport = SelectedSportId;
                tournament.Type = SelectedTourType;
                tournament.Organization = org;
                tournament.EntryType = EntryType;
                //Load or not, if the id is zero , set it's status to new.
                tournament.Status  = currentTournamentId == 0 ? TournamentStatus.New : tournament.Status;

                DbRepoTournament dbrepoTour = new DbRepoTournament(dbconn, org);
                //Save the tournament to db
                await dbrepoTour.SetAsync(tournament);
                //Save tournament id

                if (_sessionProxy is not null)  _sessionProxy.TournamentId =  tournament.Id;

            }
            await dbconn?.CloseAsync()!;

        }

        //Save to session
        if (_sessionProxy is not null) _sessionProxy.EntryType = EntryType;

        if (EntryType == 1)
            return NextPage("/TournamentFormatTeams");
        else if (EntryType == 2)
            return NextPage("/TournamentFormatPlayer");

        return Page();
    }

    private async Task LoadPage()
    {
        //uri query parameters could contain
        //key "new" equaling 1 meaning
        //a new tournament is added.

        var queryParamNew = this.HttpContext.Request.Query.ContainsKey("new") ? this.HttpContext.Request.Query["new"] : StringValues.Empty;
        

        if ( !StringValues.Empty.Equals(queryParamNew) && queryParamNew.First() == "1" && _sessionProxy is not null)
        {
            //New tournament, specified by id equaling
            //zero.
            _sessionProxy.TournamentId = 0;

        }

        var scope = _serviceProvider.CreateScope();

        var dbconn = scope.ServiceProvider.GetService<DbConnection>();
        dbconn!.ConnectionString = _config.GetConnectionString("deuce_local");
        await dbconn!.OpenAsync();

        //Load page options from the from the database
        DbRepoSport dbRepoSport = new DbRepoSport(dbconn);
        Sports = await dbRepoSport.GetList();

        DbRepoTournamentType dbRepoTourType = new DbRepoTournamentType(dbconn);

        TournamentTypes = await dbRepoTourType.GetList();

        // this.LoadFromSession();
        //Load from database
        //Set default vales;

        Organization organization = new Organization() { Id = 1, Name = "testing" };

        Tournament? currentTour = await this.GetCurrentTournament(dbconn);
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
        await dbconn!.CloseAsync();


    }

}