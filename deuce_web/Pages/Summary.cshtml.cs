using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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

   public string? Error { get; set; }


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

         //Validate tournament
         //Show /Hide the start button 
         ResultTournamentAction resultVal = new();
         if (_tourGateway is not null) resultVal = await _tourGateway.ValidateCurrentTournament();

         if ((resultVal?.Status ?? ResultStatus.Error) == ResultStatus.Error)
            Error = resultVal?.Message ?? "";


      }
      catch (Exception)
      {

      }
      finally
      {

      }

      return Page();

   }

   public async Task<IActionResult> OnPost()
   {
      //Load page values
      await LoadPage();
      //Check which action was posted.
      string? strAction = Request.Form["action"];

      //Case insensitive check for action taken
      if (String.Compare(strAction ?? "", "start", true) == 0)
      {
         //Create matches, permuations and rounds.
         if (_tourGateway is not null)
         {
            //Make the schedule for the tournament.
            //It's saved to the database
            var actionResult = await _tourGateway.StartTournament();
            //Go back to the tournaments listing
            if (actionResult.Status == ResultStatus.Ok)
               return Redirect(this.HttpContext.Request.PathBase + "/Tournaments");
            else
            {
               //Could not create shedule.
               //Display error.
               Error = actionResult.Message;
            }
         }

      }
      else if (String.Compare(strAction ?? "", "edit", true) == 0)
      {
         //Goto the edit screen screen
         return Redirect(this.HttpContext.Request.PathBase + "/TournamentDetail");

      }
      else if (String.Compare(strAction ?? "", "scores", true) == 0)
      {
         //Goto the edit screen screen
         return Redirect(this.HttpContext.Request.PathBase + "/Scoring");
      }
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