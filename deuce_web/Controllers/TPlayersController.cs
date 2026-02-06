using System.Data.Common;
using deuce;
using deuce.ext;
using deuce_web;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public class TPlayersController : WizardController
{
    private readonly ILogger<TPlayersController> _log;
    private readonly DbRepoRecordTeamPlayer _dbRepoRecordTeamPlayer;
    private readonly DbRepoTeam _dbRepoTeam;
    private readonly DbRepoPlayer _dbRepoPlayer;
    private readonly TeamRepo _teamRepo;
    private readonly FormReaderPlayersTeams _adaptorTeams;
    private readonly FormReaderIndList _adaptorInd;
    private readonly TeamValidator _validatorTeams;
    private readonly TeamValidatorInd _validatorInd;
    private readonly TeamSyncTeams _syncTeams;
    private readonly TeamSyncInd _syncInd;


    private readonly DbConnection _dbconnection;

    public string Error { get; set; } = "";
    public List<List<SelectListItem>>? SelectMember { get; set; } = new();


    public TPlayersController(ILogger<TPlayersController> log, IServiceProvider sp,
    IConfiguration config, IHandlerNavItems handlerNavItems, DbRepoRecordTeamPlayer dbRepoRecordTeamPlayer, DbRepoTeam dbRepoTeam,
     FormReaderPlayersTeams adaptorTeams, FormReaderIndList adaptorInd, TeamRepo teamRepo, DbRepoPlayer dbRepoPlayer, DbConnection dbconn,
     TeamValidator teamV, TeamValidatorInd teamVInd, TeamSyncTeams syncTeams, TeamSyncInd syncInd) : base(handlerNavItems, sp, config)
    {
        _log = log;
        _dbRepoRecordTeamPlayer = dbRepoRecordTeamPlayer;
        _dbRepoTeam = dbRepoTeam;
        _adaptorTeams = adaptorTeams;
        _teamRepo = teamRepo;
        _dbRepoPlayer = dbRepoPlayer;
        _dbconnection = dbconn;
        _adaptorInd = adaptorInd;
        _validatorTeams = teamV;
        _validatorInd = teamVInd;
        _syncTeams = syncTeams;
        _syncInd = syncInd;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            //Menus and the back button
            await LoadPage(_model, true);

            // Check if organization id is greater than zero
                _model.Error = (_sessionProxy?.OrganizationId ?? 0) <= 0 ? "Organization must be selected before proceeding with player management.":"";
                _model.IsNextButtonEnabled = (_sessionProxy?.OrganizationId ?? 0) > 0;
            
        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message);
        }

        string viewName = _model.Tournament.EntryType == (int)deuce.EntryType.Individual ? "Individual" : "Index";
        return View(viewName, _model); // Assuming you have a corresponding view named "Index"
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ViewModelTournamentWizard model)
    {
        FormUtils.DebugOut(this.Request.Form);
        IFormReaderPlayers adaptor = _model.Tournament.EntryType == (int)deuce.EntryType.Individual ?
        _adaptorInd : _adaptorTeams;


        //Read teams from the displayed form
        List<Team> teams = adaptor.Parse(this.Request.Form, _model.Tournament);

        //Validate teams before saving:
        //Team must have players
        //A player cannot appear more than once.

        ITeamValidator validator = _model.Tournament.EntryType == (int)deuce.EntryType.Individual ?
        _validatorInd : _validatorTeams;
        var isvalidTeams = validator.Check(teams, _model.Tournament);
        //Warning or error
        if (isvalidTeams.Result != RetCodeTeamAction.Success)
        {
            Error = isvalidTeams.Result == RetCodeTeamAction.Error ? isvalidTeams?.Message ?? "" : "";
            _model.Teams = _model.Tournament.EntryType == (int)deuce.EntryType.Individual ? new() : teams;
            await LoadPage(_model, false);
            string viewName = _model.Tournament.EntryType == (int)deuce.EntryType.Individual ? "Individual" : "Index";
            return View(viewName, _model); // Assuming you have a corresponding view named "Index"
        }

        //Organization DTO
        Organization organization = new Organization() { Id = _sessionProxy?.OrganizationId ?? 1 };
        //Assign to repos

        //Assign references to the team dbrepo
        _dbRepoTeam.Organization = organization;
        _dbRepoTeam.TournamentId = _model.Tournament.Id;

        //----------------------------------------
        //Sync between form and db
        //----------------------------------------

        //Get teams from db
        List<Team> teamsInDB = await _teamRepo.GetListAsync(_model.Tournament.Id);

        ITeamSync sync = _model.Tournament.EntryType == (int)deuce.EntryType.Individual ?
            _syncInd : _syncTeams;

        sync.Run(teams, teamsInDB, _model.Tournament, _dbconnection);
        //Get the next navigation item
        //Get the next navigation item
        var nextNavItem = NextPage("");
        if (nextNavItem is not null)
        {

            //Redirect to the next page
            return RedirectToAction(nextNavItem.Action, nextNavItem.Controller);
        }

        return View("Index", _model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTeam(ViewModelTournamentWizard model)
    {
        FormUtils.DebugOut(this.Request.Form);
        //Menus and the back button
        model.ShowBackButton = _showBackButton;
        model.BackPage = _backPage;
        model.NavItems = new List<NavItem>(this._handlerNavItems?.NavItems ?? Enumerable.Empty<NavItem>());

        //Read teams from the displayed form - use appropriate adaptor based on tournament type
        IFormReaderPlayers adaptor = _model.Tournament.EntryType == (int)deuce.EntryType.Individual ?
            _adaptorInd : _adaptorTeams;
        model.Teams = adaptor.Parse(this.Request.Form, model.Tournament);

        var formAdaptor = new FormReaderPlayersList();
        var formTeams = formAdaptor.Parse(this.Request.Form, model.Tournament);

        // Add new teams (for individual tournaments, these are teams with 1 player each)
        foreach (var newTeam in formTeams)
        {
            if (newTeam != null)
            {
                newTeam.Index = model.Teams.Count;
                // For individual tournaments, set the team name to the player's name
                if (_model.Tournament.EntryType == (int)deuce.EntryType.Individual && newTeam.Players.Any())
                {
                    var player = newTeam.Players.First();
                    newTeam.Label = player.ToString();
                }
                model.Teams.Add(newTeam);
            }
        }

        //Add new team/player, don't need to validate and save to db yet. Return;
        await LoadPage(model, false);
        string viewName = _model.Tournament.EntryType == (int)deuce.EntryType.Individual ? "Individual" : "Index";
        return View(viewName, model);
    }
    /// <summary>
    /// Load teams from the database and mark players that are already in a team.
    /// </summary>
    /// <param name="model">ViewModel</param>
    /// <param name="loadDB">true to load teams from the database</param>
    /// <returns></returns>
    private async Task LoadPage(ViewModelTournamentWizard model, bool loadDB = true)
    {
        //Tournament format parameters
        //Load tournament for team size

        model.Tournament.Id = _sessionProxy?.TournamentId ?? 0;

        try
        {

            
            //TODO: Filter by organization?
            Filter filterPlayer = new() { TournamentId = 0 };
            //Players registered for the tournament
            model.Players = (await _dbRepoPlayer.GetList(filterPlayer)) ?? new();            //Set tournament properties for this page
            model.Tournament.Details.TeamSize = _sessionProxy?.TeamSize ?? 2;
            model.Tournament.EntryType = _sessionProxy?.EntryType ?? (int)deuce.EntryType.Team;

            //For GETS
            if (loadDB)
            {

                //Load teams and players from the db
                Filter filterTeamPlayer = new Filter() { ClubId = _sessionProxy?.OrganizationId ?? 0, TournamentId = model.Tournament.Id };
                //Get the listing of players for the tournament.
                List<RecordTeamPlayer> tourTeamPlayersRec = await _dbRepoRecordTeamPlayer.GetList(filterTeamPlayer);
                //Create the team /player graph
                TeamRepo teamRepo = new TeamRepo(tourTeamPlayersRec);
                model.Teams = teamRepo.ExtractFromRecordTeamPlayer();

                //Set player visibility
            }


            //Remove players from the list that are already in a team
            foreach (Team team in model.Teams ?? new List<Team>())
            {
                foreach (Player player in model.Players)
                {
                    var hasPlayer = team.HasPlayer(player);
                    if (hasPlayer) player.Team = team;
                }
            }

            //Set title and selection message
            model.Title = model.Tournament.Details.TeamSize > 1 ? "Teams" : "Players";
            model.SelMsg = model.Tournament.Details.TeamSize > 1 ? "Select players that are in a team using checkboxes, add then press \"Add Team\". Repeat until all players are in a team" : "Select players using checkboxes, to add them to the tournament";


        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message);
        }
    }

    public async Task<IActionResult> DeleteTeam(ViewModelTournamentWizard model)
    {
        //Menus and the back button
        model.ShowBackButton = _showBackButton;
        model.BackPage = _backPage;
        model.NavItems = new List<NavItem>(this._handlerNavItems?.NavItems ?? Enumerable.Empty<NavItem>());

        //Get teams form the submitted form
        model.Teams = _adaptorTeams.Parse(HttpContext.Request.Form, model.Tournament);

        var formReader = new FormReaderPlayersDeleteTeams();
        var deletedTeams = formReader.Parse(HttpContext.Request.Form, model.Tournament);

        //Put players back in the list
        foreach (Team team in deletedTeams)
            model.Teams.RemoveAll(e => e.Id == team.Id && e.Index == team.Index);

        await LoadPage(model, false);
        return View("Index", model);
    }

    /// <summary>
    /// Reads an Excel file from the request and returns its content as a string.
    /// </summary>
    /// <returns>The content of the Excel file as a string, or an empty string if an error occurs.</returns>
    private async Task<string> ReadFileFromRequest(ViewModelTournamentWizard model)
    {
        // Check if a file was uploaded
        if (this.Request.Form.Files.Count > 0)
        {
            var file = this.Request.Form.Files[0];

            // Check if the file is valid
            if (file != null && file.Length > 0)
            {
                // Get the file name and content type
                string fileName = file.FileName;
                string contentType = file.ContentType;

                // Check if the file is an Excel file based on content type or file extension
                if (contentType == "application/vnd.ms-excel" ||
                contentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                 || fileName.ToLower().EndsWith(".xls")
                 || fileName.ToLower().EndsWith(".xlsx")
                 || contentType == "text/csv"
                 || fileName.ToLower().EndsWith(".csv"))
                {
                    try
                    {
                        // Read the contents of the Excel file
                        using (var stream = file.OpenReadStream())
                        {
                            // You might want to actually read the stream here and process it
                            // For example:
                            // using (var reader = new StreamReader(stream))
                            // {
                            //     return await reader.ReadToEndAsync();
                            // }
                        }
                        return "File processed successfully"; // Or return the content if you read it
                    }
                    catch (Exception ex)
                    {
                        Error = $"Error reading Excel file: {ex.Message}";
                        await LoadPage(model, false);
                        return "";
                    }
                }
                else
                {
                    Error = "Invalid file format. Please upload an Excel file.";
                    await LoadPage(model, false);
                    return "";
                }
            }
        }

        return "";
    }

}