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
   public readonly DbRepoTournament _dbrepoTournament;
   public readonly DbRepoTournamentDetail _dbrepoTournamentDetail;
   // For page values
   private Tournament? _tournament;
   private TournamentDetail? _tournamentDetail;

   public Tournament? Tournament { get => _tournament; }
   public TournamentDetail? TournamentDetail { get => _tournamentDetail; }




   //Page values

   public SummaryPageModel(ILogger<SummaryPageModel> log, ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config,
    ITournamentGateway tgateway, SessionProxy sessionProxy, ILookup lookup, DisplayToHTML displayToHTML,
    DbRepoTournament dbRepoTournament, DbRepoTournamentDetail dbRepoTournamentDetail) 
    : base(handlerNavItems, sp, config, tgateway, sessionProxy)

   {
      _log = log;
      _lookup = lookup;
      _displayToHTML = displayToHTML;
      _dbrepoTournament = dbRepoTournament;
      _dbrepoTournamentDetail = dbRepoTournamentDetail;
   }

   public async Task<IActionResult> OnGet()
   {
      try
      {
         await LoadPage();
      }
      catch(Exception)
      {
         
      }
      finally
      {
         
      }

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

      //Filter to this tournament
      Filter filter = new Filter()
      {
         TournamentId = _sessionProxy?.TournamentId ?? 0,
         ClubId = myOrg.Id
      };

      var listOfTournament = await _dbrepoTournament.GetList(filter);

      //Load tournament details
      var listOfTournamentDetails = await _dbrepoTournamentDetail.GetList(filter);

      //Set page values
      _tournament = listOfTournament.FirstOrDefault();
      _tournamentDetail = listOfTournamentDetails.FirstOrDefault();

   }

   
}