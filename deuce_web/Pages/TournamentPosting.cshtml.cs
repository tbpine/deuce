using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Entry costs
/// </summary>
public class TournamentPostingPageModel : BasePageModelWizard
{
   private readonly ILogger<TournamentPostingPageModel> _log;

   public string? Fee { get; set; }

   //Page values

   public TournamentPostingPageModel(ILogger<TournamentPostingPageModel> log, IHandlerNavItems handlerNavItems,
   IConfiguration cfg, IServiceProvider sp)
   : base(handlerNavItems, sp, cfg)
   {
      _log = log;
   }

   public async Task<IActionResult> OnGet()
   {
      //Have to query for posting fee
      Fee = $"10.00";
      return Page();
   }

   public async Task<IActionResult> OnPost()
   {
      //Check which option was selected
      //Goto the account page  index page if
      //they are waiting for more entries

      //Goto payment page if posting.
      
      return NextPage("");

   }

}