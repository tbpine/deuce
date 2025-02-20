using System.Data.Common;
using System.Reflection;
using System.Text;
using deuce;
using deuce.ext;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Entry costs
/// </summary>
public class SummaryPageModel : BasePageModelAcc
{
   private readonly ILogger<SummaryPageModel> _log;
   public readonly ILookup _lookup;
   public readonly DisplayToHTML _displayToHTML;
   // For page values
   private Tournament? _tournament;
   private TournamentDetail? _tournamentDetail;

   public Tournament? Tournament { get => _tournament; }
   public TournamentDetail? TournamentDetail { get => _tournamentDetail; }




   //Page values

   public SummaryPageModel(ILogger<SummaryPageModel> log, ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config,
    ITournamentGateway tgateway, SessionProxy sessionProxy, ILookup lookup, DisplayToHTML displayToHTML) : base(handlerNavItems, sp, config, tgateway, sessionProxy)

   {
      _log = log;
      _lookup = lookup;
      _displayToHTML = displayToHTML;
   }

   public async Task<IActionResult> OnGet()
   {
      await LoadPage();

      return Page();
   }

   public IActionResult OnPost()
   {

      return Page();

   }

   private async Task LoadPage()
   {
      //Temp organization for now,
      Organization myOrg = new() { Id = 1, Name = "myOrg" };
      //Load the current tourament

      using var scope = _serviceProvider.CreateScope();
      using var dbconn = scope.ServiceProvider.GetService<DbConnection>();
      //Bad connection object
      if (dbconn is null) return;
      //Open connection
      dbconn.ConnectionString = _config.GetConnectionString("deuce_local");
      await dbconn.OpenAsync();

      //Create repo to load tournament details
      DbRepoTournament dbRepoTournament = new(dbconn, myOrg);
      //Filter to this tournament
      Filter filter = new Filter()
      {
         TournamentId = _sessionProxy?.TournamentId ?? 0,
         ClubId = myOrg.Id
      };

      var listOfTournament = await dbRepoTournament.GetList(filter);

      //Load tournament details
      DbRepoTournamentDetail dbRepoTournamentDetail = new(dbconn, myOrg);
      var listOfTournamentDetails = await dbRepoTournamentDetail.GetList(filter);

      //Set page values

      _tournament = listOfTournament.FirstOrDefault();
      _tournamentDetail = listOfTournamentDetails.FirstOrDefault();

   }

   
}