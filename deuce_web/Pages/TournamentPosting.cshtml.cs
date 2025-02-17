using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using deuce;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Entry costs
/// </summary>
public class TournamentPostingPageModel : BasePageModelWizard
{
   private readonly ILogger<TournamentPostingPageModel> _log;
   private readonly ICacheMaster _cache;

   public string? Fee { get; set; }

   public string? Error { get; set; }

   //Page values

   public TournamentPostingPageModel(ILogger<TournamentPostingPageModel> log, IHandlerNavItems handlerNavItems,
   IConfiguration cfg, IServiceProvider sp, ICacheMaster cacheMaster)
   : base(handlerNavItems, sp, cfg)
   {
      _log = log;
      _cache = cacheMaster;
   }

   public async Task<IActionResult> OnGet()
   {
      await LoadPage();

      return Page();
   }

   public async Task<IActionResult> OnPost()
   {


      //Check which option was selected
      //Goto the account page  index page if
      //they are waiting for more entries

      string? valposting = this.HttpContext.Request.Form["posting"];
      using var scope = _serviceProvider.CreateScope();
      using var dbconn = scope.ServiceProvider.GetService<DbConnection>();

      if (dbconn is null) return Page();

      //Load page values
      await LoadPage();

      dbconn.ConnectionString = _config.GetConnectionString("deuce_local");

      if (string.Compare(valposting ?? "", "post", true) == 0)
      {
         //Creat schedule and scores for tourament.

         Organization myOrg = new() { Id = _sessionProxy?.OrganizationId ?? 1 };
         //Select tournament
         var srcTour = await GetCurrentTournament(dbconn);
         if (srcTour is not null)
         {
            TournamentOrganizer organizer = new(srcTour, dbconn, myOrg);
            //*********************************************
            //| Schedule creation here
            //*********************************************

            var results = await organizer.Run();

            //Check results 
            if (results.RetCode == RetCodeScheduling.Error)
            {
               //Error creating tourment
               Error = results.LastError;
            }
            else
               return NextPage("");
         }


         //Move to account index page.
      }


      return Page();

   }

   private async Task LoadPage()
   {
      double dlbFee = await _cache.GetDouble("fee");
      //Set page value
      Fee = dlbFee.ToString("C2");

   }

}