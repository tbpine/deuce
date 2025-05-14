using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using deuce;
using deuce_web;
using deuce_web.ext;


public class TFormatPlayerController : WizardController
{
    private readonly ILogger<TournamentFormatPlayerPageModel> _log;
    // private readonly IFormValidator _formValidator;
    private readonly ICacheMaster _cache;
    private readonly DbRepoTournamentDetail _dbRepoTournamentDetail;
    private readonly DbRepoTournament _dbRepoTournament;

    public TFormatPlayerController(ILogger<TournamentFormatPlayerPageModel> log, IServiceProvider sp,
    IConfiguration cfg, IHandlerNavItems hNavItems, ICacheMaster cache,
    DbRepoTournamentDetail dbRepoTournamentDetail,DbRepoTournament dbRepoTournament) 
    : base(hNavItems, sp, cfg)
    {
        _log = log;
        // _formValidator = formValidator;
        // _formValidator.Page = this;
        _cache = cache;
        _dbRepoTournamentDetail= dbRepoTournamentDetail;
        _dbRepoTournament = dbRepoTournament;
    }

    public async Task<IActionResult> Index()
    {
        ViewModelTournamentWizard model = new ();
        model.ShowBackButton = _showBackButton;
        model.BackPage = _backPage;
        model.NavItems = new List<NavItem>(this._handlerNavItems?.NavItems ?? Enumerable.Empty<NavItem>());

                //No validation done on loaded values.

        Organization thisOrg = new() { Id = _sessionProxy.OrganizationId , Name = "" };

        //Set the title for this page 
        //to the selected sport
        var sports = await _cache.GetList<Sport>(CacheMasterDefault.KEY_SPORTS);

        if (_sessionProxy?.TournamentId > 0)
        {
            //Load the tournament details 
            //from the database
            Filter filter = new() { TournamentId = _sessionProxy?.TournamentId??0 };
            model.TournamentDetail = (await _dbRepoTournamentDetail.GetList(filter))?.FirstOrDefault()
            ?? new()
            {

            };

            model.Tournament  = (await _dbRepoTournament.GetList(new Filter(){TournamentId = _sessionProxy?.TournamentId??0})).FirstOrDefault()??
            new(){};

            int sportId = model.Tournament?.Sport ?? 1;

            var sport = sports?.Find(e => e.Id == sportId);

            model.Title = sport?.Label ?? "";


        }

        PopulateSelectLists(model);

        return View(model);
 
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ViewModelTournamentWizard model)
    {
        model.Tournament.Id = _sessionProxy.TournamentId;
        await _dbRepoTournamentDetail.SetAsync(model.TournamentDetail);
        //Save to the session
        _sessionProxy.TeamSize = 1; 

        // Find the next navigation item
        var nextNavItem = this.NextPage("");
        if (nextNavItem != null)
        {
            // Redirect to the next page
            return RedirectToAction(nextNavItem.Action, nextNavItem.Controller);
        }
        // Optionally redirect to next step or show confirmation
        return RedirectToAction("Index");
        
    }
    private void PopulateSelectLists(ViewModelTournamentWizard model)
    {

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