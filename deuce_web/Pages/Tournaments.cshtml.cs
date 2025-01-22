using System.Data.Common;
using deuce;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

/// <summary>
/// 
/// </summary>
public class TournamentsPageModel : AccBasePageModel
{
    private readonly ILogger<TournamentsPageModel> _log;
    private List<Tournament>? _tournaments;

    public List<Tournament>? Tournaments { get=>_tournaments; }


    public TournamentsPageModel(ILogger<TournamentsPageModel> log,  ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config)
    :base(handlerNavItems, sp,  config)
    {
        _log = log;
    }

    public async Task<IActionResult> OnGetAsync()
    { 
        try
        {
            //Load tournament for this organization
            await LoadPage();

        }
        catch(Exception ex)
        {
            _log.LogError(ex.Message);
        }

        return Page();
    }


    public  IActionResult OnPostAsync()
    {
        string? strTourId = this.Request.Form["TournamentId"];
        int tourId = int.TryParse(strTourId, out tourId) ? tourId : 0;
        HttpContext.Session.SetInt32("CurrentTournament", tourId);

        return Redirect( this.Request.PathBase + "/TournamentDetail");
    }

    private async Task LoadPage()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            //Db connection
            var dbconn = scope.ServiceProvider.GetRequiredService<DbConnection>();
            if (dbconn is null) return;
            dbconn.ConnectionString = _config.GetConnectionString("deuce_local");
            await dbconn.OpenAsync();
            //get a list of tournaments
            Organization thisOrg= new Organization(){ Id = 1, Name="testing"};

            DbRepoTournamentList dbRepoTourList = new(dbconn, thisOrg);
            Filter filter= new Filter(){ ClubId = thisOrg.Id};
            _tournaments =  await dbRepoTourList.GetList(filter);

            await dbconn.CloseAsync();

        }

    }



}