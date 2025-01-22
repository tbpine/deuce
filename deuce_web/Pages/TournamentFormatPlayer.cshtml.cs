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
public class TournamentFormatPlayerPageModel : BasePageModel
{
    private readonly ILogger<TournamentFormatPlayerPageModel> _log;
    private readonly IFormValidator _formValidator;
    public string? Title { get; set; }
    public string? Error { get; set; }

    [BindProperty]
    public int NoEntries { get; set; }

    [BindProperty]
    public int Games { get; set; }


    [BindProperty]
    public int Sets { get; set; }


    [BindProperty]
    public int CustomGames { get; set; }


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
    IConfiguration cfg, IFormValidator formValidator, IHandlerNavItems hNavItems) : base(hNavItems, sp, cfg)
    {
        _log = log;
        _formValidator = formValidator;
        _formValidator.Page = this;
    }

    public async Task<IActionResult> OnGet()
    {

        //Load tournament details
        //If there's a current tournament
        try
        {
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
            //Save what you can to the session
            //Form validation.
            //Check required form values
            string err = "";
            if (!ValidateForm(ref err))
            {
                Error = err;// GetErrorMessage(_formValidator.ErrorElement ?? "");
                            // this.SaveToSession();
                return await LoadPage();
            }

            // this.SaveToSession();

            //No Error, hide the error message on page.
            Error = String.Empty;

            int currentTourId = _sessionProxy?.TournamentId ?? 0;
            if (currentTourId > 0)
            {
                TournamentDetail tourDetails = new()
                {
                    TournamentId = currentTourId,
                    NoEntries = NoEntries,
                    Games = Games,
                    Sets = Sets,
                    CustomGames = CustomGames

                };
                Organization thisOrg = new Organization(){Id=1, Name="testing"};
                //Save to DB
                var scope = _serviceProvider.CreateScope();
                var dbconn = scope.ServiceProvider.GetService<DbConnection>();
                
                if (dbconn is not null)
                {
                    dbconn.ConnectionString = _config.GetConnectionString("deuce_local");
                    await dbconn.OpenAsync();
                    DbRepoTournamentDetail dbRepoTourDetail = new (dbconn, thisOrg);
                    await dbRepoTourDetail.SetAsync(tourDetails);
                    await dbconn.CloseAsync();

                }

            }

        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message);
        }



        return NextPage("");

    }


    private bool ValidateForm(ref string err)
    {
        if (NoEntries < 2)
        {
            err = "Total players for this tournament must be greater than 2 (and a valid number) !";
            return false;
        }
        

        if (Games == 99 && CustomGames == 0)
        {
            err = "Invalid number of custom games per set";
            return false;
        }


        return true;

    }


    private async Task<IActionResult> LoadPage()
    {
        // this.LoadFromSession();

        Organization thisOrg = new() { Id = 1, Name = "testing" };

        var scope = _serviceProvider.CreateScope();
        var dbconn = scope.ServiceProvider.GetService<DbConnection>();
        dbconn!.ConnectionString = _config.GetConnectionString("deuce_local");
        await dbconn.OpenAsync();

        DbRepoSport dbRepoSport = new(dbconn);
        var sports = await dbRepoSport.GetList();

        int currentTourId = _sessionProxy?.TournamentId ?? 0;

        if (currentTourId > 0)
        {
            DbRepoTournamentDetail repoTourDetail = new(dbconn, organization: thisOrg);
            Filter filter = new() { TournamentId = currentTourId };
            var tourDetail = (await repoTourDetail.GetList(filter))?.FirstOrDefault();

            if (tourDetail is not null)
            {
                //Set page values
                NoEntries = tourDetail.NoEntries;
                Games = tourDetail.Games;
                Sets = tourDetail.Sets;
                CustomGames = tourDetail.CustomGames;

            }

            var tour = await GetCurrentTournament(dbconn);

            int sportId = tour?.Sport ?? 1;

            var sport = sports.Find(e => e.Id == sportId);

            Title = sport?.Label ?? "";


        }

        await dbconn.CloseAsync();
        return Page();
    }
}