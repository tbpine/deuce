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
    private readonly DbRepoTournamentList _dbrepoTournamentList;
    private readonly ICacheMaster _cache;

    private List<Tournament>? _tournaments;
    //For lookups
    private List<Interval>? _intervals;
    private List<TournamentType>? _tourTypes;

    public List<Tournament>? Tournaments { get => _tournaments; }

    public List<Interval>? Intervals { get => _intervals; }
    public List<TournamentType>? TourTypes { get => _tourTypes; }


    public TournamentsPageModel(ILogger<TournamentsPageModel> log, ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config,
    ITournamentGateway tgateway, SessionProxy sessionProxy, ICacheMaster cache, DbRepoTournamentList dbrepoTournamentList)
    : base(handlerNavItems, sp, config, tgateway, sessionProxy)
    {
        _log = log;
        _cache = cache;
        _dbrepoTournamentList = dbrepoTournamentList;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            //Load tournament for this organization
            await LoadPage();

        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message);
        }

        return Page();
    }


    public IActionResult OnPostAsync()
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

        if (action == "Edit")
            return Redirect(this.Request.PathBase + "/TournamentDetail");
        else if (action == "Scores")
            return Redirect(this.Request.PathBase + "/Scoring");
        else if (action == "Summary")
            return Redirect(this.Request.PathBase + "/Summary");

        return Page();
    }

    private async Task LoadPage()
    {
        //get a list of tournaments
        Organization thisOrg = new Organization() { Id = 1, Name = "testing" };

        //Get interval for labels
        _intervals = await _cache.GetList<Interval>(CacheMasterDefault.KEY_INTERVALS);

        //Get labels for tournament type i.e Round Robbin , Knockout etc..
        _tourTypes = await _cache.GetList<TournamentType>(CacheMasterDefault.KEY_TOURNAMENT_TYPES);

        Filter filter = new Filter() { ClubId = thisOrg.Id };

        //DTOs for touraments
        _tournaments = await _dbrepoTournamentList.GetList(filter);


    }



}