using System.Data.Common;
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
public class TournamentFormatPageModel : PageModel
{
    private readonly ILogger<TournamentFormatPageModel> _log;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;
    private readonly IFormValidator _formValidator;
    public string? Title { get; set; }
    public string? Error { get; set; }
    
    [BindProperty]
    public string? GamesPerSet { get; set; }
    
    [BindProperty]
    public string? TeamSize { get; set; }
    
    [BindProperty]
    public string? Sets { get; set; }

    [BindProperty]
    public string? NoSingles { get; set; }

    [BindProperty]
    public string? NoDoubles { get; set; }

    [BindProperty]
    public int NoPlayers { get; set; }

    [BindProperty]
    public int CustomNoGames { get; set; }

    [BindProperty]
    public int CustomTeamSize { get; set; }

    [BindProperty]
    public int CustomSingles { get; set; }

    [BindProperty]
    public int CustomDoubles { get; set; }

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

    public TournamentFormatPageModel(ILogger<TournamentFormatPageModel> log, IServiceProvider sp,
    IConfiguration cfg, IFormValidator formValidator)
    {
        _log = log;
        _serviceProvider = sp;
        _config = cfg;
        _formValidator = formValidator;
        _formValidator.Page = this;
    }

    private readonly string[] _valueKeys = new string[] {
        "NoPlayers", "GamesPerSet", "custom_no_games", "team_size", "sets", "custom_team_size",
        "no_singles", "no_doubles", "custom_singles", "custom_doubles"
    };

    public async Task<IActionResult> OnGet()
    {

        var scope = _serviceProvider.CreateScope();
        var dbconn = scope.ServiceProvider.GetService<DbConnection>();
        dbconn!.ConnectionString = _config.GetConnectionString("deuce_local");
        await dbconn.OpenAsync();

        DbRepoSport dbRepoSport = new(dbconn);
        var sports = await dbRepoSport.GetList();

        int sportId = this.HttpContext.Session.GetInt32("sport") ?? 0;
        int tournamentType = this.HttpContext.Session.GetInt32("tournament_type") ?? 0;

        var sport = sports.Find(e => e.Id == sportId);

        Title = sport?.Label ?? "";
        
        this.LoadFromSession();

        return Page();

    }

    public IActionResult OnPost()
    {


        //Save what you can to the session
        //Form validation.
        //Check required form values
        string err = "";
        if (!ValidateForm(ref err))
        {
            Error = err;// GetErrorMessage(_formValidator.ErrorElement ?? "");
            this.SaveToSession();
            return Page();
        }
        
        this.SaveToSession();

        //No Error, hide the error message on page.
        Error = String.Empty;

        //Everything is fine, proceed to
        //the tournament schedule page.

        return Redirect("/TournamentSchedule");

    }


    private bool ValidateForm(ref string err)
    {
        if (NoPlayers < 2)
        {
            err = "Total players for this tournament must be greater than 2 (and a valid number) !";
            return false;
        }

        if (String.IsNullOrEmpty(GamesPerSet))
        {
            err = "Select or specify how many games per set (Games per set *)";
            return false;
        }

        if (GamesPerSet == "99" && CustomNoGames == 0)
        {
            err = "Invalid number of custom games per set";
            return false;
        }

        if (String.IsNullOrEmpty(TeamSize))
        {
            err = "Select or specify no of players in a team (Team Size *)";
            return false;
        }


        if (TeamSize == "99" && CustomTeamSize == 0)
        {
            err = "Specify a team size";
            return false;
        }

        if (String.IsNullOrEmpty(Sets))
        {
            err = "Select number of sets played per match (Sets *)";
            return false;
        }

        if (String.IsNullOrEmpty(NoSingles))
        {
            err = "Select or specify how many singles are played between teams (No Singles *)";
            return false;
        }

        if (NoSingles == "99" && CustomSingles == 0)
        {
            err = "Specify a team size";
            return false;
        }

        if (String.IsNullOrEmpty(NoDoubles))
        {
            err = "Select or specify how many doubles are played between teams (No Doubles *)";
            return false;
        }

        if (NoDoubles == "99" && CustomDoubles == 0)
        {
            err = "Invalid number of custom doubles specified played between teams";
            return false;
        }

        return true;

    }


}