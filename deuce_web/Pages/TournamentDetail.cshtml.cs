using System.Data.Common;
using deuce;
using deuce_web.ext;
using iText.Bouncycastle.Crypto;
using iText.StyledXmlParser.Jsoup.Parser;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

/// <summary>
/// 
/// </summary>
public class TournamentDetailPageModel : BasePageModelWizard
{
    private readonly ILogger<TournamentDetailPageModel> _log;
    private readonly ICacheMaster _cache;
    private readonly DbRepoTournament _dbRepoTournament;
    private readonly DbRepoTournamentValidation _dbRepoTournamentValidation;

    public IEnumerable<Sport>? Sports { get; set; }
    public IEnumerable<TournamentType>? TournamentTypes { get; set; }


    [BindProperty]
    public int SelectedSportId { get; set; }

    [BindProperty]
    public int SelectedTourType { get; set; }

    [BindProperty]
    public int EntryType { get; set; }

    [BindProperty]
    public string TournamentLabel { get; set; } = "";
    //Form validation
    public bool Validated { get; set; }

    public string NameValidation { get; set; } = "";

    public TournamentDetailPageModel(ILogger<TournamentDetailPageModel> log, IServiceProvider sp,
    IConfiguration config, IHandlerNavItems hNavItems, ICacheMaster cacheMaster, DbRepoTournament dbrepoTour,
    DbRepoTournamentValidation dbRepoTournamentValidation) : base(hNavItems, sp, config)
    {
        _log = log;
        _cache = cacheMaster;
        _dbRepoTournament = dbrepoTour;
        _dbRepoTournamentValidation = dbRepoTournamentValidation;
    }

    public async Task<IActionResult> OnGet()
    {
        Validated = false;
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
        bool pageIsValid = await ValidatePage();
        
        Validated = true;

        if (!pageIsValid)
        {
            //Load values
            await LoadPage();
            //Set errors
            TournamentLabel = "";
            return Page();
        }

        //Save page properties to session
        //Todo: Move manual form values

        //Load the current tournament
        //.Set the status for new tournament
        Tournament? tournament = (_sessionProxy?.TournamentId ?? 0) > 0 ?
            (await _dbRepoTournament.GetList(new Filter(){TournamentId = _sessionProxy?.TournamentId??0})).FirstOrDefault() : null;
        if (tournament is null) tournament = new();

        //Load the current tournament id
        int currentTournamentId = _sessionProxy?.TournamentId ?? 0;
        Organization org = new Organization() { Id = 1, Name = "testing" };

        tournament.Id = currentTournamentId;
        tournament.Label = TournamentLabel;
        tournament.Sport = SelectedSportId;
        tournament.Type = SelectedTourType;
        tournament.Organization = org;
        tournament.EntryType = EntryType;
        //Load or not, if the id is zero , set it's status to new.
        tournament.Status = currentTournamentId == 0 ? TournamentStatus.New : tournament.Status;

        
        //Save the tournament to db
        await _dbRepoTournament.SetAsync(tournament);
        //Save tournament id

        if (_sessionProxy is not null) 
        {
            _sessionProxy.TournamentId = tournament.Id;
            //Teams or Individuals
            _sessionProxy.EntryType = EntryType;
        }


        if (EntryType == (int)deuce.EntryType.Team)
            return NextPage("/TournamentFormatTeams");
        else if (EntryType == (int)deuce.EntryType.Individual)
            return NextPage("/TournamentFormatPlayer");

        return Page();
    }

    private async Task LoadPage()
    {
        //Load page options from the from the database
        Sports = await _cache.GetList<Sport>(CacheMasterDefault.KEY_SPORTS);
        TournamentTypes = await _cache.GetList<TournamentType>(CacheMasterDefault.KEY_TOURNAMENT_TYPES);

        //uri query parameters could contain
        //key "new" equaling 1 meaning
        //a new tournament is added.

        var queryParamNew = this.HttpContext.Request.Query.ContainsKey("new") ? this.HttpContext.Request.Query["new"] : StringValues.Empty;


        if (!StringValues.Empty.Equals(queryParamNew) && queryParamNew.First() == "1" && _sessionProxy is not null)
        {
            //New tournament, specified by id equaling
            //zero.
            _sessionProxy.TournamentId = 0;
        }

        if ((_sessionProxy?.TournamentId??0) == 0)
        {
            //Default values

            SelectedSportId = 1;
            SelectedTourType = 1;
            TournamentLabel = "";
            EntryType = 1;

            return;

        }

        // this.LoadFromSession();
        //Load from database
        //Set default vales;

        var listOfTours = await _dbRepoTournament.GetList(new Filter() { TournamentId = _sessionProxy?.TournamentId ?? 0 });

        Tournament? currentTour = listOfTours.FirstOrDefault();
        if (currentTour is not null)
        {
            SelectedSportId = currentTour.Sport;
            SelectedTourType = currentTour.Type;
            TournamentLabel = currentTour.Label ?? TournamentLabel;
            EntryType = currentTour.EntryType;
        }


    }

    private async Task<bool> ValidatePage()
    {
        //Reset validation
        NameValidation = "";

        //Check that the label is valid
        //in the database
        Filter filter = new () { TournamentLabel = TournamentLabel};
        //Validation is done in the first record.
        var valResult = (await _dbRepoTournamentValidation.GetList(filter)).FirstOrDefault();

        if (!(valResult?.IsValid??false))
        {
            //There's a tournament with the same name
            NameValidation = "required";
            TournamentLabel = "";
            return false;
        }

        return true;

    }
}