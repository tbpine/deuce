using System.Data.Common;
using System.Threading.Tasks;
using deuce;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

/// <summary>
/// Entry costs
/// </summary>
public class TournamentPricePageModel : BasePageModelWizard
{
   private readonly ILogger<TournamentPricePageModel> _log;
   private readonly DbRepoTournament _dbRepoTournament;
   private readonly DbRepoTournamentFee _dbRepoTourFee;
   //Page values

   [BindProperty]
   public string? Fee { get; set; }

   public bool Validated { get; set; }

   public TournamentPricePageModel(ILogger<TournamentPricePageModel> log, IHandlerNavItems handlerNavItems,
   IConfiguration cfg, IServiceProvider sp, DbRepoTournament dbRepoTournament, DbRepoTournamentFee dbRepoTourFee)
   : base(handlerNavItems, sp, cfg)
   {
      _log = log;
      _dbRepoTournament = dbRepoTournament;
      _dbRepoTourFee = dbRepoTourFee;
   }

   public async Task<IActionResult> OnGet()
   {
      //Entries not validated
      Validated = false;

      //Select the current tourament
      //to get the fee charged.

      var currentTournament = (await _dbRepoTournament.GetList(new Filter() { TournamentId = _sessionProxy?.TournamentId ?? 0 })).FirstOrDefault();

      if (currentTournament is not null)
      {
         //Set the fee.
         Fee = currentTournament.Fee.ToString("F2");
      }

      return Page();
   }

   public async Task<IActionResult> OnPost()
   {
      Validated = true;

      if (!Validate()) return Page();

      //DTO (Data transfer object)
      //Tournament
      decimal dFee = decimal.TryParse(Fee, out dFee) ? dFee : 0;
      Tournament tempTour = new()
      {
         Id = _sessionProxy?.TournamentId ?? 0,
         Fee = (double)dFee
      };


      await _dbRepoTourFee.SetAsync(tempTour);


      return NextPage("");

   }

   /// <summary>
   /// Check page values
   /// </summary>
   /// <returns>True if page values are correct</returns>
   private bool Validate()
   {
      decimal dPrice = 0M;
      if (!String.IsNullOrEmpty(Fee) && !decimal.TryParse(Fee??"", out dPrice))
      {
         ModelState.AddModelError("Fee", "Please enter a valid fee.");
         return false;
      }
      return true;
   }
}