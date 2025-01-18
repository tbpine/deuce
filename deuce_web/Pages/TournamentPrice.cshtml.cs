using System.Data.Common;
using deuce;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

/// <summary>
/// 
/// </summary>
public class TournamentPricePageModel : BasePageModel
{
   private readonly ILogger<TournamentPricePageModel> _log;
   
   public TournamentPricePageModel(ILogger<TournamentPricePageModel> log, IHandlerNavItems handlerNavItems,
   IConfiguration cfg, IServiceProvider sp)
   : base(handlerNavItems, sp, cfg)
   {
      _log = log;
   }

   public IActionResult OnGet()
   {
        return Page();
   }

   public  IActionResult OnPost()
   {
        return NextPage("");
        
   }
}