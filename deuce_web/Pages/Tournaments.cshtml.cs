using System.Data.Common;
using deuce;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

/// <summary>
/// 
/// </summary>
public class TournamentsPageModel : BasePageModelAcc
{
    private readonly ILogger<TournamentsPageModel> _log;
    private List<Tournament>? _tournaments;
    //For lookups
    private List<Interval>? _intervals;
    private List<TournamentType>? _tourTypes;

    public List<Tournament>? Tournaments { get=>_tournaments; }

    public List<Interval>? Intervals{ get =>_intervals;}
    public List<TournamentType>? TourTypes { get =>_tourTypes;}


    public TournamentsPageModel(ILogger<TournamentsPageModel> log,  ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config,
    ITournamentGateway tgateway, SessionProxy sessionProxy)
    :base(handlerNavItems, sp,  config, tgateway, sessionProxy)
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
        //Edit the tourament, Enter scores
        //or goto a summary page ( to start the tournament).
        //Controlled by action.
        string? strTourId = this.Request.Form["TournamentId"];
        string? action = this.Request.Form["Action"];
        
        int tourId = int.TryParse(strTourId, out tourId) ? tourId : 0;
        HttpContext.Session.SetInt32("CurrentTournament", tourId);

        //Either edit scores for 
        //a tournament or edit it (depending on the link clicked)

        if(action == "Edit")
            return Redirect( this.Request.PathBase + "/TournamentDetail");
        else if (action == "Scores")
            return Redirect( this.Request.PathBase + "/Scoring"); 
        else if (action == "Summary")
            return Redirect( this.Request.PathBase + "/Summary"); 

        return Page();
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

            //Get interval for labels
            DbRepoInterval dbRepoInterval = new DbRepoInterval(dbconn);
            _intervals  = await dbRepoInterval.GetList();
            
            //Get labels for tournament type i.e Round Robbin , Knockout etc..
            DbRepoTournamentType dbRepoTourType = new (dbconn);
            _tourTypes = await dbRepoTourType.GetList();

            DbRepoTournamentList dbRepoTourList = new(dbconn, thisOrg);
            Filter filter= new Filter(){ ClubId = thisOrg.Id};

            //DTOs for touraments
            _tournaments =  await dbRepoTourList.GetList(filter);

            await dbconn.CloseAsync();

        }

    }



}