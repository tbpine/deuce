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

   private List<SelectListItem> _selectInterval = new List<SelectListItem>()
   {
      new SelectListItem("Select Interval", ""),
      new SelectListItem("Does not repeat", "0"),
      new SelectListItem("Weekly", "4"),
      new SelectListItem("Fortnightly", "5"),
      new SelectListItem("Monthly", "6")
   };

   public List<SelectListItem> SelectInterval { get=>_selectInterval;}

   [BindProperty]
   public int RepeatInterval { get; set; }

   [BindProperty]
   public string? StartDate { get; set; }

   public TournamentSchedulePageModel(ILogger<TournamentSchedulePageModel> log, IHandlerNavItems handlerNavItems)
   : base(handlerNavItems)
   {
      _log = log;
   }

   public ActionResult OnGet()
   {
      this.LoadFromSession();
      return Page();
   }
}