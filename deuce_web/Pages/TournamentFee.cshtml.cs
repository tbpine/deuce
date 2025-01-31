using System.Data.Common;
using System.Threading.Tasks;
using deuce;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

/// <summary>
/// Entry costs
/// </summary>
public class TournamentPricePageModel : BasePageModel
{
   private readonly ILogger<TournamentPricePageModel> _log;

   //Page values

   [BindProperty]
   public string? Fee { get; set; }

   public bool Validated { get; set; }

   public TournamentPricePageModel(ILogger<TournamentPricePageModel> log, IHandlerNavItems handlerNavItems,
   IConfiguration cfg, IServiceProvider sp)
   : base(handlerNavItems, sp, cfg)
   {
      _log = log;
   }

   public async Task<IActionResult> OnGet()
   {
      //Entries not validated
      Validated = false;

      //Select the current tourament
      //to get the fee charged.
            var scope = _serviceProvider.CreateScope();
      var dbconn = scope.ServiceProvider.GetRequiredService<DbConnection>();
      if (dbconn is not null)
      {
         dbconn.ConnectionString = _config.GetConnectionString("deuce_local");
         await dbconn.OpenAsync();

         var currentTournament = await GetCurrentTournament(dbconn);

         if (currentTournament is not null)
         {
            //Set the fee.
            Fee = currentTournament.Fee.ToString("F2");
         }

         await dbconn.CloseAsync();
      }

      return Page();
   }

   public async Task<IActionResult> OnPost()
   {
      Validated = true;

      if (!Validate()) return Page();

      //DTO (Data transfer object)
      //Tournamenty
      decimal dFee = decimal.TryParse(Fee, out dFee) ? dFee : 0;
      Tournament tempTour = new()
      {
         Id = _sessionProxy?.TournamentId ?? 0,
         Fee = (double)dFee
      };

      //Save to the database
      //Make scope connection
      var scope = _serviceProvider.CreateScope();
      var dbconn = scope.ServiceProvider.GetRequiredService<DbConnection>();

      if (dbconn is not null)
      {
         dbconn.ConnectionString = _config.GetConnectionString("deuce_local");
         await dbconn.OpenAsync();

         DbRepoTournamentFee repoFee = new(dbconn);
         await repoFee.SetAsync(tempTour);
         dbconn.Close();
      }


      return NextPage("");

   }

   /// <summary>
   /// Check page values
   /// </summary>
   /// <returns>True if page values are correct</returns>
   private bool Validate()
   {
      decimal dPrice = decimal.TryParse(Fee, out dPrice) ? dPrice : 0;
      if ((double)dPrice == 0d)
      {
         Fee = "";
         return false;
      }
      return true;
   }
}