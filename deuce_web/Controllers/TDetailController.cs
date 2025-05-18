using Microsoft.AspNetCore.Mvc;
using deuce;
using deuce.ext;
using System.Diagnostics;


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
        //Load form values from the database
        var rowsTournament = await _dbRepoTournament.GetList(new Filter() { TournamentId = _model.Tournament.Id });
        var rowTournament = rowsTournament.FirstOrDefault();
        //Copy values to the model and set the default values

        _model.Tournament.Sport = rowTournament?.Sport ?? 1;
        _model.Tournament.Type = rowTournament?.Type ?? 1;
        _model.Tournament.Label = rowTournament?.Label ?? "";
        _model.Tournament.EntryType = rowTournament?.EntryType ?? 1;
        _model.Tournament.TeamSize = rowTournament?.TeamSize ?? 2;

        return View("Index", _model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ViewModelTournamentWizard src)
    {
        
        //The data pipe line is this:
        //1. Dependecies injections (menus)
        //2. Session
        //3. Form values

        Tournament obj = _model.Tournament;
        //Set form values
        obj.Label = src.Tournament.Label;
        obj.Sport = src.Tournament.Sport;
        obj.Type = src.Tournament.Type;
        obj.EntryType = src.Tournament.EntryType;

        bool pageIsValid = await Validate(obj, _model);
        _model.Validated = true;

        if (!pageIsValid)
        {
            //Set errors
            obj.Label = "";
            return View("Index", _model);
        }


        //Load or not, if the id is zero , set it's status to new.

        obj.Status = obj.IsNew() ? TournamentStatus.New : obj.Status;
        //Save the tournament to db
        await _dbRepoTournament.SetAsync(obj);
        //Save tournament id
        _sessionProxy.TournamentId = obj.Id;
        //Teams or Individuals
        _sessionProxy.EntryType = obj.EntryType;

        var nextNavItem = NextPage("");

        if (nextNavItem is not null)
        {
            //No next page
            return RedirectToAction(nextNavItem?.Action, nextNavItem?.Controller);
        }

        return View("Index", _model);
    }

    private async Task<bool> Validate(Tournament obj, ViewModelTournamentWizard model)
    {
        //Reset validation

        model.NameValidation = "";

        //Check that the label is valid
        //in the database
        Filter filter = new() { TournamentLabel = obj.Label };
        //Validation is done in the first record.
        var valResult = (await _dbRepoTournamentValidation.GetList(filter)).FirstOrDefault();

        if (!(valResult?.IsValid ?? false) && _sessionProxy?.TournamentId <= 0)
        {
            //There's a tournament with the same name
            model.NameValidation = "required";
            obj.Label = "";
            return false;
        }

        return true;

    }


}