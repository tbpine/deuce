using System.Data.Common;
using deuce;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

/// <summary>
/// 
/// </summary>
public class TournamentVenuePageModel : BasePageModelWizard
{
   private readonly ILogger<TournamentVenuePageModel> _log;
   private readonly DbRepoTournament _dbrepoTournament;
   private readonly DbRepoTournamentDetail _dbrepoTournamentDetail;
   private readonly DbRepoVenue _dbRepoVenue;


   [BindProperty]
   public string? Street { get; set; }


   [BindProperty]
   public string? Suburb { get; set; }

   [BindProperty]
   public string? PostCode { get; set; }

   [BindProperty]
   public string? State { get; set; }


   [BindProperty]
   public string? Country { get; set; }

   public bool Validated { get; set; }
   public string? ErrElement { get; set; }

   public List<SelectListItem> _countries = new();

   public List<SelectListItem> Countries { get => _countries; }

   public TournamentVenuePageModel(ILogger<TournamentVenuePageModel> log, IHandlerNavItems handlerNavItems,
   IConfiguration cfg, IServiceProvider sp, DbRepoTournament dbrepoTournament, DbRepoTournamentDetail dbrepoTournamentDetail, DbRepoVenue dbrepoVenue)
   : base(handlerNavItems, sp, cfg)
   {
      _log = log;
      _dbrepoTournament = dbrepoTournament;
      _dbrepoTournamentDetail = dbrepoTournamentDetail;
      _dbRepoVenue = dbrepoVenue;      
   }

   public async Task<IActionResult> OnGet()
   {
      try
      {
         Validated = false;
            
         return await LoadPage();
      }
      catch (Exception ex)
      {
         _log.LogError(ex.Message);
      }
      return Page();
   }

   public async  Task<IActionResult> OnPost()
   {
      //Load Page values
      await LoadPage();
      
      //Page validation
      Validated = true;
      if (!ValidatePage()) return Page();

      //Tournament DTO
      Tournament tourDTO = new Tournament() { Id = _sessionProxy?.TournamentId ?? 0 };

      //Convert postcode
      int postCode = int.TryParse(this.PostCode, out postCode) ? postCode : 0;

      //Make the model class
      TournamentVenue venue = new TournamentVenue()
      {
         Id = -1,
         Tournament = tourDTO,
         Street = this.Street ?? "",
         Suburb = this.Suburb ?? "",
         PostCode = postCode,
         State = this.State ?? "",
         Country = this.Country ?? ""
      };


      //Save venue details using 
      // a db repo
      //Save teams to the database
      var scope = _serviceProvider.CreateScope();
      var dbconn = scope.ServiceProvider.GetRequiredService<DbConnection>();
      if (dbconn is not null)
      {
         dbconn.ConnectionString = _config.GetConnectionString("deuce_local");
         await dbconn.OpenAsync();

         DbRepoVenue repoVenue = new DbRepoVenue(dbconn);
         repoVenue.Set(venue);
         dbconn.Close();
      }

      return NextPage("");

   }

   /// <summary>
   /// Load page content
   /// </summary>
   /// <returns></returns>
   private async Task<IActionResult> LoadPage()
   {
      var firstOption = new SelectListItem("", "");
      firstOption.Selected = true;
      
      _countries.Add(firstOption);
      _countries.Add(new SelectListItem("Australia", "aus"));
      _countries.Add(new SelectListItem("US", "us"));
      _countries.Add(new SelectListItem("UK", "uk"));
      _countries.Add(new SelectListItem("New Zealand", "nz"));

      //Load the venue where this tournament is
      //to be held.
      var scope = _serviceProvider.CreateScope();
      var dbconn = scope.ServiceProvider.GetRequiredService<DbConnection>();
      if (dbconn is not null)
      {
 
         Filter filter = new Filter()
         {
            TournamentId = _sessionProxy?.TournamentId ?? 0,
            ClubId = 1
         };

         var listOfVenues = await _dbRepoVenue.GetList(filter);
         dbconn.Close();
         
         //Set values
         if (listOfVenues.Count >0)
         {
            TournamentVenue loadedVenue = listOfVenues[0];
            //Set binded properties that will
            //be displayed on the page.
            Street = loadedVenue.Street;
            Suburb = loadedVenue.Suburb;
            State = loadedVenue.State;
            PostCode = loadedVenue.PostCode.ToString();
            Country = loadedVenue.Country;

         }
      }



      return Page();
   }

   /// <summary>
   /// True if all entries are valid
   /// </summary>
   /// <param name="err">Error message to display</param>
   /// <returns></returns>
   private bool ValidatePage()
   {
      //Check that the  post code is is valid
      int iPostCode = int.TryParse(PostCode?.Trim()??"", out iPostCode) ? iPostCode : 0;
      if (iPostCode == 0)
      {
         PostCode = "";
         return false;
      }

      //Check country selection
      if (String.IsNullOrEmpty(Country))
      {
         return false;
      }


      return true;
   }
}