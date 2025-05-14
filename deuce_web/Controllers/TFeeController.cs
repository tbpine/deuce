using System.Data.Common;
using System.Threading.Tasks;
using deuce;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

/// <summary>
/// Entry costs
/// </summary>
public class TFeeController : WizardController
{
    private readonly ILogger<TFeeController> _log;
    private readonly DbRepoTournament _dbRepoTournament;
    private readonly DbRepoTournamentFee _dbRepoTourFee;
    //Page values

    public TFeeController(ILogger<TFeeController> log, IHandlerNavItems handlerNavItems,
    IConfiguration cfg, IServiceProvider sp, DbRepoTournament dbRepoTournament, DbRepoTournamentFee dbRepoTourFee)
    : base(handlerNavItems, sp, cfg)
    {
        _log = log;
        _dbRepoTournament = dbRepoTournament;
        _dbRepoTourFee = dbRepoTourFee;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        //Entries not validated
        _model.Validated = false;

        //Select the current tourament
        //to get the fee charged.

        _model.Tournament = (await _dbRepoTournament.GetList(new Filter() { TournamentId = _sessionProxy?.TournamentId ?? 0 })).FirstOrDefault()
        ??new() {Fee = 0};


        return View(_model);
    }

    [HttpPost]
    public async Task<IActionResult> Save(ViewModelTournamentWizard model)
    {
        //Entries validated
        _model.Validated = true;

        if (!Validate()) return View();

        //DTO (Data transfer object)
        //Tournament
        
        Tournament tempTour = new()
        {
            Id = _sessionProxy?.TournamentId ?? 0,
        };
 

        await _dbRepoTourFee.SetAsync(tempTour);

        //Get the next nav item
        //and redirect to it.
        var nextNavItem = NextPage("");
        if (nextNavItem is not null)
        {
            //Set the next page
            _sessionProxy?.SetNextPage(nextNavItem.Page);
            //Redirect to the next page
            return RedirectToAction(nextNavItem.Action, nextNavItem.Controller);
        }
        else
        {
            //No next page
            //Redirect to the index page
            return RedirectToAction("Index", "Tournament");
        }
        return View(_model);

    }

    /// <summary>
    /// Check page values
    /// </summary>
    /// <returns>True if page values are correct</returns>
    private bool Validate(ViewModelTournamentWizard model)
    {
        
        return true;
    }
   
}