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
   private readonly DbRepoVenue _dbRepoVenue;
   private readonly ICacheMaster  _cache;


   [BindProperty]
   public string? Street { get; set; }


   [BindProperty]
   public string? State { get; set; }

   [BindProperty]
   public int CountryCode { get; set; }

   public bool Validated { get; set; }
   public string? ErrElement { get; set; }

   public List<SelectListItem> _countries = new();

   public List<SelectListItem> Countries { get => _countries; }

   public TournamentVenuePageModel(ILogger<TournamentVenuePageModel> log, IHandlerNavItems handlerNavItems,
   IConfiguration cfg, IServiceProvider sp, DbRepoVenue dbrepoVenue, ICacheMaster cache)
   : base(handlerNavItems, sp, cfg)
   {
      _log = log;
      _dbRepoVenue = dbrepoVenue;
      _cache = cache;
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

   public async Task<IActionResult> OnPost()
   {
      //Load Page values
      await LoadPage();

      //Page validation
      Validated = true;
      if (!ValidatePage()) return Page();

      //Tournament DTO
      Tournament tourDTO = new Tournament() { Id = _sessionProxy?.TournamentId ?? 0 };


      //Make the model class
      TournamentVenue venue = new TournamentVenue()
      {
         Id = -1,
         Tournament = tourDTO,
         Street = this.Street ?? "",
         Suburb = "",
         PostCode = 0,
         State = this.State ?? "",
         CountryCode = this.CountryCode
      };


      //Save venue details using 
      // a db repo
      //Save teams to the database

      _dbRepoVenue.Set(venue);

      
      int entryType = _sessionProxy?.EntryType??(int)deuce.EntryType.Team;

        if (entryType == (int)deuce.EntryType.Team)
            return NextPage("/TournamentFormatTeams");
        else if (entryType == (int)deuce.EntryType.Individual)
            return NextPage("/TournamentFormatPlayer");

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

      //Load countries from the cache.
      var listOfCountries =  await _cache.GetList<Country>(CacheMasterDefault.KEY_ENTRY_COUNTRIES);
      foreach(Country country in listOfCountries??new())
         _countries.Add(new SelectListItem(country.Name, country.Code.ToString()));
         
      //Load the venue where this tournament is
      //to be held.

      Filter filter = new Filter()
      {
         TournamentId = _sessionProxy?.TournamentId ?? 0,
         ClubId = 1
      };

      var listOfVenues = await _dbRepoVenue.GetList(filter);

      //Set values
      if (listOfVenues.Count > 0)
      {
         TournamentVenue loadedVenue = listOfVenues[0];
         //Set binded properties that will
         //be displayed on the page.
         Street = loadedVenue.Street;
         State = loadedVenue.State;
         CountryCode = loadedVenue.CountryCode;

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

      //Check country selection
      if (CountryCode < 0)
      {
         return false;
      }


      return true;
   }
}