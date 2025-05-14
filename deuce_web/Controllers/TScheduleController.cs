using deuce;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


public class TScheduleController : WizardController
{
    private readonly ILogger<TScheduleController> _log;
    private readonly DbRepoTournament _dbrepoTournament;
    private readonly DbRepoTournamentProps _dbrepoTournamentProps;
    public TScheduleController(ILogger<TScheduleController> log, IHandlerNavItems handlerNavItems,
    IConfiguration cfg, IServiceProvider sp, DbRepoTournament dbrepoTournament, DbRepoTournamentProps dbRepoTournamentProps)
    : base(handlerNavItems, sp, cfg)
    {
        _log = log;
        _dbrepoTournament = dbrepoTournament;
        _dbrepoTournamentProps = dbRepoTournamentProps;
    }

    [HttpGet]
    public async Task<ActionResult> Index()
    {
        _model.Validated = false;

        try
        {
            await LoadPage(_model);
        }
        catch (Exception)
        { }
        finally { }

        //Set page values
        return View(_model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ViewModelTournamentWizard formValues)
    {
        try
        {
            //Set back page properties
            _model.Validated = true;

            //Check entries.
            if (!ValidatePage())
            {
                return View("Index", _model);
            }

            //Load the current tournament from the database
            Organization thisOrg = new Organization() { Id = _sessionProxy?.OrganizationId ?? 0, Name = "" };
            //Save to the database.
            await _dbrepoTournamentProps.SetAsync(formValues.Tournament);


        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message);
        }
        //Get the next navigation page
        var navItem = NextPage("");
        if (navItem is not null)
        {
            return RedirectToAction(navItem.Action, navItem.Controller);
        }

        return View("Index", formValues);

    }

    /// <summary>
    /// Load page values
    /// </summary>
    /// <returns>Nothing</returns>
    private async Task LoadPage(ViewModelTournamentWizard model)
    {
        model.SelectInterval = new List<SelectListItem>()
        {
            new SelectListItem("Select Interval", ""),
            new SelectListItem("Does not repeat", "0"),
            new SelectListItem("Weekly", "4"),
            new SelectListItem("Fortnightly", "5"),
            new SelectListItem("Monthly", "6")
        };

        //Load tournament schedule here
        model.Tournament = (await _dbrepoTournament.GetList(new Filter() { TournamentId = _sessionProxy?.TournamentId ?? 0 })).FirstOrDefault() ??
            new Tournament()
            {
                Id = _sessionProxy?.TournamentId ?? 0,
                Start = DateTime.Now,
                Interval = 4
            };




    }

    /// <summary>
    /// Check page values
    /// </summary>
    /// <returns>true if all values on the page are correct</returns>
    private bool ValidatePage()
    {
        return true;
    }
}