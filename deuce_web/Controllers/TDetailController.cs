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

    public async Task<IActionResult> Index(int id)
    {
        ViewModelTournamentWizard viewModel = new ViewModelTournamentWizard();
        viewModel.Validated = false;
        //Load page options from the from the database
        viewModel.Sports = await _cache.GetList<Sport>(CacheMasterDefault.KEY_SPORTS) ?? new();
        viewModel.TournamentTypes = await _cache.GetList<TournamentType>(CacheMasterDefault.KEY_TOURNAMENT_TYPES) ?? new();

        //uri query parameters could contain
        //key "new" equaling 1 meaning
        //a new tournament is added.

        _sessionProxy.TournamentId = 0;

        if ((_sessionProxy?.TournamentId ?? 0) == 0)
        {
            //Default values
            viewModel.Tournament.Sport = 1;
            viewModel.Tournament.Type = 1;
            viewModel.Tournament.Label = "";
            viewModel.Tournament.EntryType = 1;

        }
        else
        {
            var listOfTours = await _dbRepoTournament.GetList(new Filter() { TournamentId = _sessionProxy.TournamentId });
            viewModel.Tournament = listOfTours.FirstOrDefault() ?? new();
        }

        return View(viewModel);
    }

    public async Task<IActionResult> Save(ViewModelTournamentWizard viewModel)
    {
        bool pageIsValid = await Validate(viewModel);
        viewModel.Validated = true;

        if (!pageIsValid)
        {
            //Set errors
            viewModel.Tournament.Label = "";
            return View(viewModel);
        }

        Organization org = new Organization() {Id = _sessionProxy.OrganizationId, Name = ""};
        //Load or not, if the id is zero , set it's status to new.
        viewModel.Tournament.Status = viewModel.Tournament.Id == 0 ? TournamentStatus.New : viewModel.Tournament.Status;

        //Save the tournament to db
        await _dbRepoTournament.SetAsync(viewModel.Tournament);
        //Save tournament id
        _sessionProxy.TournamentId = viewModel.Tournament.Id;
        //Teams or Individuals
        _sessionProxy.EntryType = viewModel.Tournament.EntryType;
       
        return RedirectToAction("Index", "");
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
            tournament.Label  = "";
            return false;
        }

        return true;

    }


}