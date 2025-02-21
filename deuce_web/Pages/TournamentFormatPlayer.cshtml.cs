using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using deuce;
using deuce_web;
using deuce_web.ext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

/// <summary>
/// 
/// </summary>
public class TournamentFormatPlayerPageModel : BasePageModelWizard
{
    private readonly ILogger<TournamentFormatPlayerPageModel> _log;
    private readonly IFormValidator _formValidator;
    private readonly ICacheMaster _cache;
    private readonly DbRepoTournamentDetail _dbRepoTournamentDetail;
    private readonly DbRepoTournament _dbRepoTournament;

    public string? Title { get; set; }
    
    [BindProperty]
    public string? NoEntries { get; set; }

    [BindProperty]
    public string? Games { get; set; }


    [BindProperty]
    public string? Sets { get; set; }


    [BindProperty]
    public string? CustomGames { get; set; }

    public bool Validated { get; set; }


    public List<SelectListItem> SelectSets = new List<SelectListItem>()
    {
        new SelectListItem("Select Number of Sets", ""),
        new SelectListItem("1", "1"),
        new SelectListItem("2", "2"),
        new SelectListItem("3", "3"),
        new SelectListItem("4", "4"),
        new SelectListItem("5", "5")
    };

    public List<SelectListItem> SelectGamesPerSet = new List<SelectListItem>()
    {
        new SelectListItem("Number of games per set", ""),
        new SelectListItem("6 (Standard rules)", "1"),
        new SelectListItem("4 (Fast four)", "2"),
        new SelectListItem("Custom", "99")
    };

    public TournamentFormatPlayerPageModel(ILogger<TournamentFormatPlayerPageModel> log, IServiceProvider sp,
    IConfiguration cfg, IFormValidator formValidator, IHandlerNavItems hNavItems, ICacheMaster cache,
    DbRepoTournamentDetail dbRepoTournamentDetail,DbRepoTournament dbRepoTournament) : base(hNavItems, sp, cfg)
    {
        _log = log;
        _formValidator = formValidator;
        _formValidator.Page = this;
        _cache = cache;
        _dbRepoTournamentDetail= dbRepoTournamentDetail;
        _dbRepoTournament = dbRepoTournament;
    }

    public async Task<IActionResult> OnGet()
    {

        //Load tournament details
        //If there's a current tournament
        try
        {
            Validated = false;
            return await LoadPage();
        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message);
        }


        return Page();
    }

    public async Task<IActionResult> OnPost()
    {

        try
        {
            Validated = true;
            //Save what you can to the session
            //Form validation.
            //Check required form values
            if (!ValidateForm())
            {
                return Page();
            }

            // this.SaveToSession();

            //No Error, hide the error message on page.
            

            int currentTourId = _sessionProxy?.TournamentId ?? 0;
            if (currentTourId > 0)
            {
                TournamentDetail tourDetails = new()
                {
                    TournamentId = currentTourId,
                    NoEntries = int.Parse(NoEntries??"2"),
                    Games = int.Parse(Games??"1"),
                    Sets = int.Parse(Sets??"1"),
                    CustomGames = int.Parse(CustomGames??"8"),
                    TeamSize = 1

                };
                Organization thisOrg = new Organization(){Id=1, Name="testing"};
                //Save to DB
                await _dbRepoTournamentDetail.SetAsync(tourDetails);


            }

        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message);
        }



        return NextPage("");

    }


    private bool ValidateForm()
    {
        
        int iNumEntries = int.TryParse(NoEntries, out iNumEntries) ? iNumEntries : 2;
        if (iNumEntries < 2) 
        {
            NoEntries = "";
            return false;
        }

        //Check custom games
        if (string.IsNullOrEmpty(Games))
        {
            Games = "";
            return false;
        } 

        if (Games == "99")
        {
            int iCustomGame = int.TryParse(CustomGames, out iCustomGame) ? iCustomGame : 0;
            if (iCustomGame < 1)    
            {
                CustomGames = "";
                return false;
            }
            
        }

        //Sets
        if (string.IsNullOrEmpty(Sets))
        {
            Sets = "";
            return false;
        } 



        return true;

    }


    private async Task<IActionResult> LoadPage()
    {
        //No validation done on loaded values.

        Organization thisOrg = new() { Id = 1, Name = "testing" };

        //Set the title for this page 
        //to the selected sport
        var sports = await _cache.GetList<Sport>(CacheMasterDefault.KEY_SPORTS);

        int currentTourId = _sessionProxy?.TournamentId ?? 0;

        if (currentTourId > 0)
        {
            //Load the tournament details 
            //from the database
            Filter filter = new() { TournamentId = currentTourId };
            var tourDetail = (await _dbRepoTournamentDetail.GetList(filter))?.FirstOrDefault();

            //Set page values
            if (tourDetail is not null)
            {
                //Set page values
                NoEntries = tourDetail.NoEntries.ToString();
                Games = tourDetail.Games.ToString();
                Sets = tourDetail.Sets.ToString();
                CustomGames = tourDetail.CustomGames.ToString();

            }

            var tour = (await _dbRepoTournament.GetList(new Filter(){TournamentId = _sessionProxy?.TournamentId??0})).FirstOrDefault();

            int sportId = tour?.Sport ?? 1;

            var sport = sports?.Find(e => e.Id == sportId);

            Title = sport?.Label ?? "";


        }

        return Page();
    }
}