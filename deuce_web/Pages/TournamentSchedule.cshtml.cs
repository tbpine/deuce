using deuce;
using deuce_web.ext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

/// <summary>
/// 
/// </summary>
public class TournamentSchedulePageModel : BasePageModel
{
   private readonly ILogger<TournamentSchedulePageModel> _log;
   private readonly IConfiguration _config;
   private readonly IServiceProvider _service;


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
   public int RepeatInterval { get; set; }

   [BindProperty]
   public string? StartDate { get; set; }

   public TournamentSchedulePageModel(ILogger<TournamentSchedulePageModel> log, IHandlerNavItems handlerNavItems,
   IConfiguration cfg, IServiceProvider sp)
   : base(handlerNavItems)
   {
      _log = log;
      _config = cfg;
      _service = sp;
   }

   public ActionResult OnGet()
   {
      this.LoadFromSession();
      return Page();
   }

   public async Task<IActionResult> OnPost()
   {
      try
      {
         this.SaveToSession();
         //Load the current tournament from the database
         Organization thisOrg = new Organization() { Id = 1, Name = "testing" };
         var currentTour = await GetCurrentTournament(_service, _config, thisOrg);
         if (currentTour is not null)
         {
            //Set start date and interval
            DateTime tmpStartDate = DateTime.TryParse(this.StartDate, out tmpStartDate) ? tmpStartDate : DateTime.MinValue;

            currentTour.Start = tmpStartDate.Equals(DateTime.MinValue) ? DateTime.Now : tmpStartDate;

            currentTour.Interval = this.RepeatInterval;

            //Save to the database.

            await SetCurrentTournament(currentTour, _service, _config, thisOrg);
         }


         return NextPage("");
      }
      catch(Exception ex)
      {
         _log.LogError(ex.Message);
      }

      return Page();

   }
}