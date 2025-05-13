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
        var model = new ViewModelTournamentWizard();

        try
        {
            //Transfer properties from the base class
            //to the view model.
            model.ShowBackButton = _showBackButton;
            model.BackPage = _backPage;
            model.NavItems = new List<NavItem>(this._handlerNavItems?.NavItems ?? Enumerable.Empty<NavItem>());
            // LoadPage logic
            var sports = await _cache.GetList<Sport>(CacheMasterDefault.KEY_SPORTS);
            model.Tournament = (await _dbRepoTournament.GetList(new Filter() { TournamentId = _sessionProxy.TournamentId })).FirstOrDefault() ?? 
            new(){ TeamSize = 2,};

            var sport = sports?.Find(e => e.Id == (model.Tournament?.Sport ?? 0));
            model.Title = sport?.Label ?? "";
            Organization thisOrg = new() { Id = _sessionProxy.OrganizationId, Name = "testing" };
            Filter filter = new() { TournamentId = _sessionProxy.TournamentId };
            model.TournamentDetail = (await _dbRepoTournamentDetail.GetList(filter))?.FirstOrDefault() ?? new()
            {

                Games = 1,
                Sets = 1,
                NoSingles = 2,
                NoDoubles = 2,
                TeamSize = 2
            };

            model.Tournament.TeamSize = model.TournamentDetail.TeamSize;

            model.Format.NoSingles = model.TournamentDetail.NoSingles < 6 ? model.TournamentDetail.NoSingles : 99;
            model.Format.NoDoubles = model.TournamentDetail.NoDoubles < 6 ? model.TournamentDetail.NoDoubles : 99;
            model.CustomSingles = model.TournamentDetail.NoSingles < 6 ? 0 : model.TournamentDetail.NoSingles;
            model.CustomDoubles = model.TournamentDetail.NoDoubles < 6 ? 0 : model.TournamentDetail.NoDoubles;

            PopulateSelectLists(model);
            return View(model);
        }
        catch (Exception ex)
        {
            _log.Log(LogLevel.Error, ex.Message);
        }

        return View(model);
    }

    // POST: /TFormatTeam/Save
    [HttpPost]
    public async Task<IActionResult> Save(ViewModelTournamentWizard model)
    {
        // Form validation logic (adapt as needed)
        string err = "";
        if (!ValidateForm(model, ref err))
        {
            model.Error = err;
            PopulateSelectLists(model);
            return View("Index", model);
        }

        model.Error = string.Empty;

        // Save to db
        int currentTournamentId = _sessionProxy?.TournamentId ?? 0;
        if (currentTournamentId > 0)
        {
            model.TournamentDetail.NoSingles = model.Format.NoSingles < 6 ? model.Format.NoSingles : (model.CustomSingles ?? 0);
            model.TournamentDetail.NoDoubles = model.Format.NoDoubles < 6 ? model.Format.NoDoubles : (model.CustomDoubles ?? 0);
            model.Tournament.TeamSize = model.Tournament.TeamSize;

            Organization thisOrg = new() { Id = _sessionProxy?.OrganizationId ?? 1 };
            await _dbRepoTournamentDetail.SetAsync(model.TournamentDetail);

            //Set proxy value
            if (_sessionProxy is not null) _sessionProxy.TeamSize = model.Tournament.TeamSize;
        }

        // Redirect or go to next page as needed
        var nextNavItem = this.NextPage("");
        if (nextNavItem != null)
        {
            // Redirect to the next page
            return RedirectToAction(nextNavItem.Action, nextNavItem.Controller);
        }

        return View("Index");
        
    }

    private bool ValidateForm(ViewModelTournamentWizard model, ref string err)
    {
        if (model.Tournament.TeamSize < 1)
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
            new SelectListItem("4 (Fast four)", "2"),
            new SelectListItem("Custom", "99")
        };
    }
}