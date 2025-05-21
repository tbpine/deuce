using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using deuce;
using deuce_web;

/// <summary>
/// Controller for the Tournament Format Team page.
/// This controller handles the logic for displaying and saving the tournament format settings.
/// It includes methods for loading the initial page, validating the form input,
/// and saving the tournament format details.
/// </summary>
public class TFormatTeamController : WizardController
{
    private readonly ILogger<TFormatTeamController> _log;
    private readonly IFormValidator _formValidator;
    private readonly DbRepoTournamentDetail _dbRepoTournamentDetail;
    private readonly ICacheMaster _cache;
    private readonly DbRepoTournament _dbRepoTournament;

    public TFormatTeamController(ILogger<TFormatTeamController> log,
        IServiceProvider sp, IConfiguration cfg, IFormValidator formValidator,
        IHandlerNavItems hNavItems, DbRepoTournamentDetail dbRepoTournamentDetail,
        ICacheMaster cache, DbRepoTournament dbRepoTournament
    ) : base(hNavItems, sp, cfg)
    {
        _log = log;
        _formValidator = formValidator;
        _dbRepoTournamentDetail = dbRepoTournamentDetail;
        _cache = cache;
        _dbRepoTournament = dbRepoTournament;
    }

    // GET: /TFormatTeam/
    [HttpGet]
    public async Task<IActionResult> Index()
    {

        try
        {
            //Transfer properties from the base class
            //to the view model.
            // LoadPage logic
            var sports = await _cache.GetList<Sport>(CacheMasterDefault.KEY_SPORTS);
            //Load tournament  from the database
            var rowTournament = (await _dbRepoTournament.GetList(new Filter() { TournamentId = _model.Tournament.Id })).FirstOrDefault();
            //Load tournament details from the database using the dbRpepoTournamentDetail
            var rowTournamentDetail = (await _dbRepoTournamentDetail.GetList(new Filter() { TournamentId = _model.Tournament.Id })).FirstOrDefault();

            _model.Tournament.Sport = rowTournament?.Sport ?? 1;
            _model.Tournament.Details = rowTournamentDetail?? new()
            {
                Games = 1,
                Sets = 1,
                NoSingles = 2,
                NoDoubles = 2,
                TeamSize = 2
            };

            var sport = sports?.Find(e => e.Id == (_model.Tournament?.Sport ?? 1));
            _model.Title = sport?.Label ?? "";
            
            Filter filter = new() { TournamentId = _sessionProxy.TournamentId };
          

            _model.CustomSingles = _model.TournamentDetail.NoSingles < 6 ? 0 : _model.TournamentDetail.NoSingles;
            _model.CustomDoubles = _model.TournamentDetail.NoDoubles < 6 ? 0 : _model.TournamentDetail.NoDoubles;

            PopulateSelectLists(_model);
            return View(_model);
        }
        catch (Exception ex)
        {
            _log.Log(LogLevel.Error, ex.Message);
        }

        return View(_model);
    }

    // POST: /TFormatTeam/Save
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ViewModelTournamentWizard formValues)
    {

        // Form validation logic (adapt as needed)
        string err = "";
        _model.Tournament.Details.TournamentId = _model.Tournament.Id;
        
        _model.Tournament.Details.TeamSize = formValues.Tournament.Details.TeamSize < 99 ?
        formValues.Tournament.Details.TeamSize : formValues.CustomTeamSize??2;
        _model.Tournament.Details.Games = formValues.Tournament.Details.Games < 7 ?
         formValues.Tournament.Details.Games : formValues.CustomGames ?? 1;
        _model.Tournament.Details.Sets = formValues.Tournament.Details.Sets;
        _model.Tournament.Details.NoSingles = formValues.Tournament.Details.NoSingles < 99 ?
         formValues.Tournament.Details.NoSingles : formValues.CustomSingles ?? 1;
        _model.Tournament.Details.NoDoubles = formValues.Tournament.Details.NoDoubles < 99 ?
         formValues.Tournament.Details.NoDoubles : formValues.CustomDoubles ?? 1;

        if (!ValidateForm(_model, ref err))
        {
            _model.Error = err;
            PopulateSelectLists(_model);
            return View("Index", _model);
        }

        _model.Error = string.Empty;

        // Save to db
        await _dbRepoTournamentDetail.SetAsync(_model.Tournament.Details);

        //Set proxy value
        if (_sessionProxy is not null) _sessionProxy.TeamSize = _model.Tournament.Details.TeamSize;

        // Redirect or go to next page as needed
        var nextNavItem = this.NextPage("");
        if (nextNavItem != null)
        {
            // Redirect to the next page
            return RedirectToAction(nextNavItem.Action, nextNavItem.Controller);
        }

        return View("Index", _model);

    }

    private bool ValidateForm(ViewModelTournamentWizard model, ref string err)
    {
        if (model.Tournament.Details.TeamSize < 1)
        {
            err = "Select or specify no of players in a team (Team Size *)";
            return false;
        }
        if (model.Format.NoSingles < 1)
        {
            err = "Select or specify how many singles are played between teams (No Singles *)";
            return false;
        }
        if (model.Format.NoDoubles < 1)
        {
            err = "Select or specify how many doubles are played between teams (No Doubles *)";
            return false;
        }
        return true;
    }

    private void PopulateSelectLists(ViewModelTournamentWizard model)
    {
        model.SelectTeamSize = new[]
        {
            new SelectListItem("Select Team Size", ""),
            new SelectListItem("1", "1"),
            new SelectListItem("2", "2"),
            new SelectListItem("3", "3"),
            new SelectListItem("4", "4"),
            new SelectListItem("5", "5"),
            new SelectListItem("6", "6"),
            new SelectListItem("Custom", "99")
        };

        model.SelectSets = new[]
        {
            new SelectListItem("Select Number of Sets", ""),
            new SelectListItem("1", "1"),
            new SelectListItem("2", "2"),
            new SelectListItem("3", "3"),
            new SelectListItem("4", "4"),
            new SelectListItem("5", "5")
        };

        model.SelectNoGames = new[]
        {
            new SelectListItem("Number of Games", ""),
            new SelectListItem("1", "1"),
            new SelectListItem("2", "2"),
            new SelectListItem("3", "3"),
            new SelectListItem("4", "4"),
            new SelectListItem("5", "5"),
            new SelectListItem("6", "6"),
            new SelectListItem("Custom", "99")
        };

        model.SelectGamesPerSet = new[]
        {
            new SelectListItem("Number of games per set", "0"),
            new SelectListItem("6 (Standard rules)", "1"),
            new SelectListItem("4 (Fast four)","2"),
            new SelectListItem("Custom", "99")
        };
    }
}