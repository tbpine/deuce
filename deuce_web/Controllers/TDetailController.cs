using Microsoft.AspNetCore.Mvc;
using deuce;
using Microsoft.AspNetCore.Mvc.Filters;

public class TDetailController : WizardController
{
    private readonly ILogger<TDetailController> _log;
    private readonly ICacheMaster _cache;
    private readonly DbRepoTournament _dbRepoTournament;
    private readonly DbRepoTournamentValidation _dbRepoTournamentValidation;

    public TDetailController(ILogger<TDetailController> log, IServiceProvider sp,
    IConfiguration config, IHandlerNavItems hNavItems, ICacheMaster cacheMaster, DbRepoTournament dbrepoTour,
    DbRepoTournamentValidation dbRepoTournamentValidation) : base(hNavItems, sp, config)
    {
        _log = log;
        _cache = cacheMaster;
        _dbRepoTournament = dbrepoTour;
        _dbRepoTournamentValidation = dbRepoTournamentValidation;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        _model.Validated = false;
        //Load page options from the from the database
        _model.Sports = await _cache.GetList<Sport>(CacheMasterDefault.KEY_SPORTS) ?? new();
        _model.TournamentTypes = await _cache.GetList<TournamentType>(CacheMasterDefault.KEY_TOURNAMENT_TYPES) ?? new();

        var listOfTours = await _dbRepoTournament.GetList(new Filter() { TournamentId = _sessionProxy!.TournamentId });
        _model.Tournament = listOfTours.FirstOrDefault() ?? new()
        {
            Sport = 1,
            Type = 1,
            Label = "",
            EntryType = 1,
            TeamSize = 2

        };

        return View(_model);
    }

    public async Task<IActionResult> Save(ViewModelTournamentWizard viewModel)
    {
        //Copy values from the submitted form
        _model.Tournament = viewModel.Tournament;
        bool pageIsValid = await Validate(_model);
        _model.Validated = true;

        if (!pageIsValid)
        {
            //Set errors
            _model.Tournament.Label = "";
            return View(_model);
        }

        Organization org = new Organization() { Id = _sessionProxy.OrganizationId, Name = "" };
        //Load or not, if the id is zero , set it's status to new.

        _model.Tournament.Status = _model.Tournament.Id == 0 ? TournamentStatus.New : _model.Tournament.Status;
        //Save the tournament to db
        await _dbRepoTournament.SetAsync(_model.Tournament);
        //Save tournament id
        _sessionProxy.TournamentId = _model.Tournament.Id;
        //Teams or Individuals
        _sessionProxy.EntryType = _model.Tournament.EntryType;

        var nextNavItem = NextPage("");

        if (nextNavItem == null)
        {
            //No next page
            return RedirectToAction("Index");
        }

        return RedirectToAction(nextNavItem?.Action, nextNavItem?.Controller);
    }

    private async Task<bool> Validate(ViewModelTournamentWizard viewModel)
    {
        //Reset validation
        Tournament tournament = viewModel.Tournament;

        viewModel.NameValidation = "";

        //Check that the label is valid
        //in the database
        Filter filter = new() { TournamentLabel = tournament.Label };
        //Validation is done in the first record.
        var valResult = (await _dbRepoTournamentValidation.GetList(filter)).FirstOrDefault();

        if (!(valResult?.IsValid ?? false) && _sessionProxy?.TournamentId <= 0)
        {
            //There's a tournament with the same name
            viewModel.NameValidation = "required";
            tournament.Label = "";
            return false;
        }

        return true;

    }


}