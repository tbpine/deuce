using System.Data.Common;
using deuce;
using deuce_web.ext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualBasic;

/// <summary>
/// 
/// </summary>
public class TournamentSchedulePageModel : BasePageModel
{
   private readonly ILogger<TournamentSchedulePageModel> _log;
   private const string DateFormat = "dd/MM/yyyy";

   private List<SelectListItem> _selectInterval = new List<SelectListItem>()
   {
      new SelectListItem("Select Interval", ""),
      new SelectListItem("Does not repeat", "0"),
      new SelectListItem("Weekly", "4"),
      new SelectListItem("Fortnightly", "5"),
      new SelectListItem("Monthly", "6")
   };

   public List<SelectListItem> SelectInterval { get => _selectInterval; }

   [BindProperty]
   public int Interval { get; set; }

   [BindProperty]
   public string? StartDate { get; set; }

   public TournamentSchedulePageModel(ILogger<TournamentSchedulePageModel> log, IHandlerNavItems handlerNavItems,
   IConfiguration cfg, IServiceProvider sp)
   : base(handlerNavItems, sp, cfg)
   {
      _log = log;
   }

   public async Task<ActionResult> OnGet()
   {
      //Load tournament schedule here
      var currentTour = await GetCurrentTournament(null);
      if (currentTour is not null)
      {
         Interval = currentTour.Interval;
         StartDate = currentTour.Start.ToString(DateFormat);
      }

      //Defaults


      if (StartDate == DateTime.MinValue.ToString(DateFormat)) StartDate = DateTime.Now.ToString(DateFormat);
      if (Interval < 1) Interval = 4;

      //Set page values
      return Page();
   }

   public async Task<IActionResult> OnPost()
   {
      try
      {
         //Load the current tournament from the database
         Organization thisOrg = new Organization() { Id = 1, Name = "testing" };

         int currentTourId = _sessionProxy?.TournamentId ?? 0;
         if (currentTourId > 0)
         {

            //Set start date and interval
            DateTime tmpStartDate = DateTime.TryParse(StartDate, out tmpStartDate) ? tmpStartDate : DateTime.Now;
            //Partial object
            Tournament tmp = new()
            {
               Id = currentTourId,
               Start = tmpStartDate,
               Interval = Interval
            };

            var scope = _serviceProvider.CreateScope();
            var dbconn = scope.ServiceProvider.GetService<DbConnection>();
            if (dbconn is not null)
            {
               dbconn.ConnectionString = _config.GetConnectionString("deuce_local");
               await dbconn.OpenAsync();

               DbRepoTournamentProps dbRepo = new(dbconn);
               //Save to the database.
               await dbRepo.SetAsync(tmp);
            }

         }

         return NextPage("");
      }
      catch (Exception ex)
      {
         _log.LogError(ex.Message);
      }

      return Page();

   }
}