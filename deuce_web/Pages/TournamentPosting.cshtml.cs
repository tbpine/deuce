using System.Data.Common;
using System.Runtime.CompilerServices;
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
      //Get a list of settings for the tournament d
      //fee.
      using var scopee =  _serviceProvider.CreateScope();
      using var dbconn = scopee.ServiceProvider.GetRequiredService<DbConnection>();
      dbconn.ConnectionString = _config.GetConnectionString("deuce_local");
      //open connection to the database
      await dbconn.OpenAsync();

      //Load settings
      var dbRepoSettings = new DbRepoSettings(dbconn);
      var listOfSettings = await dbRepoSettings.GetList();

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