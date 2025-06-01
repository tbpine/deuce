using Microsoft.AspNetCore.Mvc;
using deuce;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Data.Common;

/// <summary>
/// Tournament controller
/// </summary>
public class TournamentController : MemberController
{

    private readonly ILogger<TournamentsPageModel> _log;
    private readonly DbRepoTournamentList _dbrepoTournamentList;
    private readonly ICacheMaster _cache;

    private readonly DbRepoTournament _dbRepoTournament;
    private readonly DbRepoTournamentDetail _dbRepoTournamentDetail;
    private readonly DisplayToHTML _displayToHTML;
    private readonly DbConnection _dbConnection;
    private readonly DbRepoRecordSchedule _dbRepoRecordSchedule;

    public TournamentController(ILogger<TournamentsPageModel> log, ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config,
    ITournamentGateway tgateway, SessionProxy sessionProxy, ICacheMaster cache, DbRepoTournamentList dbrepoTournamentList,
    DbRepoTournament dbRepoTournament, DbRepoTournamentDetail dbRepoTournamentDetail, DisplayToHTML displayToHTML, DbConnection dbConnection,
    DbRepoRecordSchedule dbRepoRecordSchedule)
    : base(handlerNavItems, sp, config, tgateway, sessionProxy)
    {
        _log = log;
        _cache = cache;
        _dbrepoTournamentList = dbrepoTournamentList;
        _dbRepoTournament = dbRepoTournament;
        _dbRepoTournamentDetail = dbRepoTournamentDetail;
        _displayToHTML = displayToHTML;
        _dbConnection = dbConnection;
        _dbRepoRecordSchedule = dbRepoRecordSchedule;
    }


    public async Task<IActionResult> Index()
    {
        //get a list of tournaments
        Organization thisOrg = new Organization()
        {
            Id = _sessionProxy.OrganizationId,
            Name = ""
        };

        //Make a ViewModelTournament object and set properties

        //Get interval for labels
        _model.Intervals = await _cache.GetList<Interval>(CacheMasterDefault.KEY_INTERVALS) ?? new();

        //Get labels for tournament type i.e Round Robbin , Knockout etc..
        _model.TournamentTypes = await _cache.GetList<TournamentType>(CacheMasterDefault.KEY_TOURNAMENT_TYPES) ?? new();

        Filter filter = new Filter() { ClubId = thisOrg.Id };

        //DTOs for touraments
        _model.Tournaments = await _dbrepoTournamentList.GetList(filter);

        return View(_model);

    }

    public async Task<IActionResult> Summary(int id)
    {
        // Retrieve tournament details based on tournamentId
        //using the getlist method
        //Load tournament details
        Filter filter = new Filter()
        {
            TournamentId = id,
            ClubId = _model.Organization.Id // Assuming organization ID is available in session
        };


        Tournament tournament = (await _tourGateway.GetTournament(id)) ?? new();
        TournamentDetail tournamentDetail = (await _dbRepoTournamentDetail.GetList(filter)).FirstOrDefault() ?? new();

        // Validate tournament
        ResultTournamentAction resultVal = new ResultTournamentAction();
        if (_tourGateway != null && tournament != null)
        {
            resultVal = await _tourGateway.ValidateCurrentTournament(tournament);

            if ((resultVal?.Status ?? ResultStatus.Error) == ResultStatus.Error)
            {
                // Handle validation error (e.g., set an error message in the model)
                _model.Error = resultVal?.Message ?? "";
            }
        }

        _model.Tournament = tournament!;
        _model.TournamentDetail = tournamentDetail;
        _model.HtmlTour = await _displayToHTML.ExtractDisplayProperty(tournament!);
        _model.HtmlTourDetail = await _displayToHTML.ExtractDisplayProperty(tournamentDetail);

        //Set session tournament id
        _sessionProxy.TournamentId = id;

        return View("Summary", _model);
    }

    public async Task<IActionResult> Start()
    {

        //Make the schedule for the tournament.
        //It's saved to the database
        var actionResult = await _tourGateway.StartTournament();
        //Go back to the tournaments listing
        if (actionResult.Status == ResultStatus.Ok)
            return RedirectToAction("Index", "Tournament");
        else
        {
            //Could not create shedule.
            //Display error.
            _model.Error = actionResult.Message;
        }
        //If there's an error, go back to the summary page
        return View("Summary", _model);
    }

   
    public IActionResult Edit()
    {
        return RedirectToAction("Index", "TDetail", new { id = _model.Tournament.Id });
    }
}