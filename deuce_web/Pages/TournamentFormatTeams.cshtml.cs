using System.Data.Common;
using System.Reflection;
using System.Xml.Linq;
using deuce;
using deuce_web;
using deuce_web.ext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

/// <summary>
/// 
/// </summary>
public class TournamentFormatTeamsPageModel : BasePageModelWizard
{
    private readonly ILogger<TournamentFormatTeamsPageModel> _log;
    private readonly IFormValidator _formValidator;
    private readonly DbRepoTournamentDetail _dbRepoTournamentDetail;
    private readonly ICacheMaster _cache;
    private readonly DbRepoTournament _dbRepoTournament;
    public string? Title { get; set; }
    public string? Error { get; set; }

    [BindProperty]
    public int Games { get; set; }

    [BindProperty]
    public int TeamSize { get; set; }

    [BindProperty]
    public int Sets { get; set; }

    [BindProperty]
    public int NoSingles { get; set; }

    [BindProperty]
    public int NoDoubles { get; set; }

    [BindProperty]
    public int CustomGames { get; set; }

    [BindProperty]
    public int CustomTeamSize { get; set; }

    [BindProperty]
    public int CustomSingles { get; set; }

    [BindProperty]
    public int CustomDoubles { get; set; }

    public string? EntryTypeLabel { get; set; }

    public List<SelectListItem> SelectTeamSize = new List<SelectListItem>()
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

    public List<SelectListItem> SelectSets = new List<SelectListItem>()
    {
        new SelectListItem("Select Number of Sets", ""),
        new SelectListItem("1", "1"),
        new SelectListItem("2", "2"),
        new SelectListItem("3", "3"),
        new SelectListItem("4", "4"),
        new SelectListItem("5", "5")
    };

    public List<SelectListItem> SelectNoGames = new List<SelectListItem>()
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

    public List<SelectListItem> SelectGamesPerSet = new List<SelectListItem>()
    {
        new SelectListItem("Number of games per set", "0"),
        new SelectListItem("6 (Standard rules)", "1"),
        new SelectListItem("4 (Fast four)", "2"),
        new SelectListItem("Custom", "99")
    };

    public TournamentFormatTeamsPageModel(ILogger<TournamentFormatTeamsPageModel> log, IServiceProvider sp,
    IConfiguration cfg, IFormValidator formValidator, IHandlerNavItems hNavItems, DbRepoTournamentDetail dbRepoTournamentDetail,
    ICacheMaster cache, DbRepoTournament dbRepoTournament) : base(hNavItems, sp, cfg)
    {
        _log = log;
        _formValidator = formValidator;
        _formValidator.Page = this;
        _dbRepoTournamentDetail = dbRepoTournamentDetail;
        _cache = cache;
        _dbRepoTournament = dbRepoTournament;
    }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            await LoadPage();
        }
        catch (Exception ex)
        {
            _log.Log(LogLevel.Error, ex.Message);
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {


        //Save what you can to the session
        //Form validation.
        //Check required form values
        string err = "";
        if (!ValidateForm(ref err))
        {
            Error = err;// GetErrorMessage(_formValidator.ErrorElement ?? "");
            return Page();
        }


        //No Error, hide the error message on page.
        Error = String.Empty;

        //Everything is fine, proceed to
        //the tournament schedule page.
        //Save to db
        int currentTournamentId = _sessionProxy?.TournamentId ?? 0;
        if (currentTournamentId > 0)
        {
            TournamentDetail tourDetail = new()
            {
                TournamentId = currentTournamentId,
                Sets = Sets,
                Games = Games,
                CustomGames = CustomGames,
                NoSingles = NoSingles < 6 ? NoSingles : CustomSingles,
                NoDoubles = NoDoubles < 6 ? NoDoubles : CustomDoubles,
                TeamSize = TeamSize
            };

            Organization thisOrg = new() { Id = _sessionProxy?.OrganizationId ?? 1 };
            await _dbRepoTournamentDetail.SetAsync(tourDetail);
        }

        return NextPage("");

    }


    private bool ValidateForm(ref string err)
    {

        if (TeamSize < 1)
        {
            err = "Select or specify no of players in a team (Team Size *)";
            return false;
        }


        if (NoSingles < 1)
        {
            err = "Select or specify how many singles are played between teams (No Singles *)";
            return false;
        }


        if (NoDoubles < 1)
        {
            err = "Select or specify how many doubles are played between teams (No Doubles *)";
            return false;
        }

        return true;

    }

    /// <summary>
    /// Get page values
    /// </summary>
    private async Task LoadPage()
    {
        var sports = await _cache.GetList<Sport>(CacheMasterDefault.KEY_SPORTS);
        //Load tournament
        var currentTour = (await _dbRepoTournament.GetList(new Filter() { TournamentId = _sessionProxy?.TournamentId ?? 0 })).FirstOrDefault();

        if (currentTour is not null)
        {

            var sport = sports?.Find(e => e.Id == (currentTour?.Sport ?? 0));

            Title = sport?.Label ?? "";

            //Load Tournament detais
            Organization thisOrg = new() { Id = 1, Name = "testing" };

            Filter filter = new() { TournamentId = currentTour.Id };
            var tourDetail = (await _dbRepoTournamentDetail.GetList(filter))?.FirstOrDefault();

            if (tourDetail is not null)
            {
                //Set page values
                Games = tourDetail.Games;
                CustomGames = tourDetail.CustomGames;
                Sets = tourDetail.Sets;
                TeamSize = tourDetail.TeamSize;
                NoSingles = tourDetail.NoSingles < 6 ? tourDetail.NoSingles : 99;
                NoDoubles = tourDetail.NoDoubles < 6 ? tourDetail.NoDoubles : 99;
                CustomSingles = tourDetail.NoSingles < 6 ? 0 : tourDetail.NoSingles;
                CustomDoubles = tourDetail.NoDoubles < 6 ? 0 : tourDetail.NoDoubles;

                return;

            }

        }
        //Default values
        Games = 1;
        Sets = 1;
        TeamSize = 2;
        NoSingles = 2;
        NoDoubles = 2;
    }

}