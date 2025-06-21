using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using deuce;
using deuce_web;
using iText.Forms.Xfdf;
using deuce.ext;

/// <summary>
/// Controller for the Tournament Format Team page.
/// This controller handles the logic for displaying and saving the tournament format settings.
/// It includes methods for loading the initial page, validating the form input,
/// and saving the tournament format details.
/// </summary>
public class TFormatTeamController : WizardController
{
    private readonly ILogger<TFormatTeamController> _log;
    private readonly IFormValidator _formValidator;
    private readonly DbRepoTournamentDetail _dbRepoTournamentDetail;
    private readonly ICacheMaster _cache;
    private readonly DbRepoTournament _dbRepoTournament;

    public TFormatTeamController(ILogger<TFormatTeamController> log,
        IServiceProvider sp, IConfiguration cfg, IFormValidator formValidator,
        IHandlerNavItems hNavItems, DbRepoTournamentDetail dbRepoTournamentDetail,
        ICacheMaster cache, DbRepoTournament dbRepoTournament
    ) : base(hNavItems, sp, cfg)
    {
        _log = log;
        _formValidator = formValidator;
        _dbRepoTournamentDetail = dbRepoTournamentDetail;
        _cache = cache;
        _dbRepoTournament = dbRepoTournament;
    }

    // GET: /TFormatTeam/
    [HttpGet]
    public async Task<IActionResult> Index(int entry_type)
    {

        try
        {
            _model.Tournament.EntryType = entry_type;

            //Transfer properties from the base class
            //to the view model.
            // LoadPage logic
            LoadPage();

            //Load tournament  from the database
            var rowTournament = (await _dbRepoTournament.GetList(new Filter() { TournamentId = _model.Tournament.Id })).FirstOrDefault();
            //Load tournament details from the database using the dbRpepoTournamentDetail
            var rowTournamentDetail = (await _dbRepoTournamentDetail.GetList(new Filter() { TournamentId = _model.Tournament.Id })).FirstOrDefault();

            _model.Tournament.Sport = rowTournament?.Sport ?? 1;
            //Default tournment details is based on tournament type
            if (rowTournamentDetail is not null) _model.Tournament.Details = rowTournamentDetail;
            else _model.Tournament.CreateDetail();

            SetCustomValues(_model, "Tournament.Details.TeamSize", "CustomTeamSize", 6);
            SetCustomValues(_model, "Tournament.Details.Games", "CustomGames", 6);
            SetCustomValues(_model, "Tournament.Details.NoSingles", "CustomSingles", 6);
            SetCustomValues(_model, "Tournament.Details.NoDoubles", "CustomDoubles", 6);

            var sports = await _cache.GetList<Sport>(CacheMasterDefault.KEY_SPORTS);

            var sport = sports?.Find(e => e.Id == (_model.Tournament?.Sport ?? 1));
            _model.Title = sport?.Label ?? "";

            Filter filter = new() { TournamentId = _sessionProxy.TournamentId };

            PopulateSelectLists(_model);

        }
        catch (Exception ex)
        {
            _log.Log(LogLevel.Error, ex.Message);
        }

        return View(GetView((EntryType)entry_type), _model);
    }

    /// <summary>
    ///  Load page options 
    /// </summary>
    /// <returns></returns>
    private void LoadPage()
    {
        PopulateSelectLists(_model);
    }

    /// <summary>
    /// Get the view name based on the entry type.
    /// </summary>
    /// <param name="entryType"></param>
    /// <returns></returns>
    private string GetView(EntryType entryType)
    {
        return entryType switch
        {
            EntryType.Individual => "Singles",
            _ => "Index"
        };
    }

    // POST: /TFormatTeam/Save
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ViewModelTournamentWizard formValues)
    {

        //If entry type is individual, set specific tournament details
        //values
        // Form validation logic (adapt as needed)
        string err = "";
        //Set details id
        _model.Tournament.Details.TournamentId = _model.Tournament.Id;

        if (_model.Tournament.EntryType == (int)EntryType.Individual)
        {
            _model.Tournament.Details.Sets = formValues.Tournament.Details.Sets;
            _model.Tournament.Details.Games = formValues.Tournament.Details.Games < 99 ? formValues.Tournament.Details.Games :
            formValues.CustomGames ?? 1;
            _model.Tournament.Details.NoSingles = 1;
            _model.Tournament.Details.NoDoubles = 0;
            _model.Tournament.Details.TeamSize = 1;
        }
        else
        {


            _model.Tournament.Details.TeamSize = formValues.Tournament.Details.TeamSize < 99 ?
            formValues.Tournament.Details.TeamSize : formValues.CustomTeamSize ?? 2;
            _model.Tournament.Details.Games = formValues.Tournament.Details.Games < 99 ?
             formValues.Tournament.Details.Games : formValues.CustomGames ?? 1;
            _model.Tournament.Details.Sets = formValues.Tournament.Details.Sets;
            _model.Tournament.Details.NoSingles = formValues.Tournament.Details.NoSingles < 99 ?
             formValues.Tournament.Details.NoSingles : formValues.CustomSingles ?? 1;
            _model.Tournament.Details.NoDoubles = formValues.Tournament.Details.NoDoubles < 99 ?
             formValues.Tournament.Details.NoDoubles : formValues.CustomDoubles ?? 1;
        }

        if (!ValidateForm(_model, ref err))
        {
            _model.Error = err;
            PopulateSelectLists(_model);
            return View("Index", _model);
        }

        _model.Error = string.Empty;

        // Save to db
        await _dbRepoTournamentDetail.SetAsync(_model.Tournament.Details);

        //Set proxy value
        if (_sessionProxy is not null) _sessionProxy.TeamSize = _model.Tournament.Details.TeamSize;

        // Redirect or go to next page as needed
        var nextNavItem = this.NextPage("");
        if (nextNavItem != null)
        {
            // Redirect to the next page
            return RedirectToAction(nextNavItem.Action, nextNavItem.Controller);
        }

        return View("Index", _model);

    }

    private bool ValidateForm(ViewModelTournamentWizard model, ref string err)
    {
        if (model.Tournament.Details.TeamSize < 1)
        {
            err = "Select or specify no of players in a team (Team Size *)";
            return false;
        }
        if (model.Format.NoSingles < 1)
        {
            err = "Select or specify how many singles are played between teams (No Singles *)";
            return false;
        }
        if (model.Format.NoDoubles < 1)
        {
            err = "Select or specify how many doubles are played between teams (No Doubles *)";
            return false;
        }
        return true;
    }

    private void PopulateSelectLists(ViewModelTournamentWizard model)
    {
        model.SelectTeamSize = new[]
        {
            new SelectListItem("Select Team Size", ""),
            new SelectListItem("1", "1"),
            new SelectListItem("2", "2"),
            new SelectListItem("3", "3"),
            new SelectListItem("4", "4"),
            new SelectListItem("5", "5"),
            new SelectListItem("6", "6"),
            new SelectListItem("Custom", "99")
        };

        model.SelectSets = new[]
        {
            new SelectListItem("Select Number of Sets", ""),
            new SelectListItem("1", "1"),
            new SelectListItem("2", "2"),
            new SelectListItem("3", "3"),
            new SelectListItem("4", "4"),
            new SelectListItem("5", "5")
        };

        model.SelectNoGames = new[]
        {
            new SelectListItem("Number of Games", ""),
            new SelectListItem("1", "1"),
            new SelectListItem("2", "2"),
            new SelectListItem("3", "3"),
            new SelectListItem("4", "4"),
            new SelectListItem("5", "5"),
            new SelectListItem("6", "6"),
            new SelectListItem("Custom", "99")
        };

        model.SelectGamesPerSet = new[]
        {
            new SelectListItem("Number of games per set", "0"),
            new SelectListItem("6 (Standard rules)", "6"),
            new SelectListItem("4 (Fast four)","4"),
            new SelectListItem("Custom", "99")
        };
    }

    /// <summary>
    /// Set custom values for selection lists.
    /// </summary>
    /// <param name="src">model</param>
    /// <param name="src">Which property can be specified</param>
    /// <param name="dest">Property containing the custom value</param>
    /// <param name="limit">Highest non custom value</param>
    private void SetCustomValues(object source, string src, string dest, int limit)
    {
        Type type = source.GetType();
        //Get the integer value from "src" property of the model
        int value = (int)(Utils.GetPropertyByPath(source, src) ?? 0);

        //If the value is greater than the limit, set src to 99
        if (value > limit)
        {
            Utils.SetPropertyByPath(source, src, 99);
            //Set the destination property to the value
            Utils.SetPropertyByPath(source, dest, value); ;
        }
        else
        {
            //Otherwise set the destination property to 0
            Utils.SetPropertyByPath(source, dest, 0);
        }


    }

}