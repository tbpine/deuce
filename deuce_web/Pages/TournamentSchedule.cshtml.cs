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
public class TournamentSchedulePageModel : BasePageModelWizard
{
   private readonly ILogger<TournamentSchedulePageModel> _log;
   private readonly DbRepoTournament _dbrepoTournament;
   private readonly DbRepoTournamentProps _dbrepoTournamentProps;
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
   public string? Interval { get; set; }

   [BindProperty]
   public string? StartDate { get; set; }

   public bool Validated { get; set; }

   public TournamentSchedulePageModel(ILogger<TournamentSchedulePageModel> log, IHandlerNavItems handlerNavItems,
   IConfiguration cfg, IServiceProvider sp, DbRepoTournament dbrepoTournament, DbRepoTournamentProps dbRepoTournamentProps)
   : base(handlerNavItems, sp, cfg)
   {
      _log = log;
      _dbrepoTournament = dbrepoTournament;
      _dbrepoTournamentProps = dbRepoTournamentProps;
   }

   public async Task<ActionResult> OnGet()
   {
      Validated = false;

      try
      {
         await LoadPage();
      }
      catch (Exception)
      { }
      finally { }

      //Set page values
      return Page();
   }

   public async Task<IActionResult> OnPost()
   {
      try
      {
         Validated = true;

         //Check entries.
         if (!ValidatePage())
         {
            return Page();
         }

         //Load the current tournament from the database
         Organization thisOrg = new Organization() { Id = 1, Name = "testing" };

         int currentTourId = _sessionProxy?.TournamentId ?? 0;
         if (currentTourId > 0)
         {

            //Set start date and interval
            DateTime tmpStartDate = DateTime.TryParse(StartDate, out tmpStartDate) ? tmpStartDate : DateTime.Now;

            //Tournament DTO
            Tournament tmp = new()
            {
               Id = currentTourId,
               Start = tmpStartDate,
               Interval = int.Parse(Interval ?? "")
            };


            //Save to the database.
            await _dbrepoTournamentProps.SetAsync(tmp);

         }

         return NextPage("");
      }
      catch (Exception ex)
      {
         _log.LogError(ex.Message);
      }

      return Page();

   }

   /// <summary>
   /// Load page values
   /// </summary>
   /// <returns>Nothing</returns>
   private async Task LoadPage()
   {
      //Load tournament schedule here
      var currentTour = (await _dbrepoTournament.GetList(new Filter(){ TournamentId = _sessionProxy?.TournamentId ??0 })).FirstOrDefault();
      if (currentTour is not null)
      {
         Interval = currentTour.Interval.ToString();
         StartDate = currentTour.Start.ToString(DateFormat);
      }
      
      //Defaults

      if (StartDate == DateTime.MinValue.ToString(DateFormat)) StartDate = DateTime.Now.ToString(DateFormat);
      if (Interval == "0") Interval = "4";

   }

   /// <summary>
   /// Check page values
   /// </summary>
   /// <returns>true if all values on the page are correct</returns>
   private bool ValidatePage()
   {
      //Check in interval
      int ival = int.TryParse(Interval, out ival) ? ival : -1;
      if (ival < 0)
      {
         Interval = "";
         return false;
      }

      return true;
   }
}