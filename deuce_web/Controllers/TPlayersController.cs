using deuce;
using deuce.ext;
using deuce_web;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public class TPlayersController : WizardController
{
    private readonly ILogger<TPlayersController> _log;
    private readonly DbRepoTournamentDetail _dbRepoTournamentDetail;
    private readonly DbRepoRecordTeamPlayer _dbRepoRecordTeamPlayer;
    private readonly DbRepoTeam _dbRepoTeam;
    private readonly DbRepoTournament _dbRepoTournament;
    private readonly DbRepoMember _dbRepoMember;
    private readonly DbRepoVenue _dbRepoVenue;
    private readonly DbRepoPlayer _dbRepoPlayer;
    private readonly TeamRepo _teamRepo;

    private readonly IFormReaderPlayers _adaptorTeams;
    private readonly ITournamentGateway _tournamentGateway;

    public string Error { get; set; } = "";
    public List<List<SelectListItem>>? SelectMember { get; set; } = new();


    public TPlayersController(ILogger<TPlayersController> log, IServiceProvider sp,
    IConfiguration config, IHandlerNavItems handlerNavItems,
    DbRepoTournamentDetail dbRepoTournamentDetail, DbRepoRecordTeamPlayer dbRepoRecordTeamPlayer, DbRepoTeam dbRepoTeam,
    DbRepoTournament dbRepoTournament, IFormReaderPlayers adaptorTeams, DbRepoMember dbRepoMember, DbRepoVenue dbRepoVenue,
    TeamRepo teamRepo, DbRepoPlayer dbRepoPlayer, ITournamentGateway tournamentGateway) : base(handlerNavItems, sp, config)
    {
        _log = log;
        _dbRepoTournamentDetail = dbRepoTournamentDetail;
        _dbRepoRecordTeamPlayer = dbRepoRecordTeamPlayer;
        _dbRepoTeam = dbRepoTeam;
        _dbRepoTournament = dbRepoTournament;
        _adaptorTeams = adaptorTeams;
        _dbRepoMember = dbRepoMember;
        _dbRepoVenue = dbRepoVenue;
        _teamRepo = teamRepo;
        _dbRepoPlayer = dbRepoPlayer;
        _tournamentGateway = tournamentGateway;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewModelTournamentWizard model = new();
        try
        {
            //Menus and the back button
            model.ShowBackButton = _showBackButton;
            model.BackPage = _backPage;
            model.NavItems = new List<NavItem>(this._handlerNavItems?.NavItems ?? Enumerable.Empty<NavItem>());

            await LoadPage(model, true);
        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message);
        }

        return View("Index", model); // Assuming you have a corresponding view named "Index"
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ViewModelTournamentWizard model)
    {
        FormUtils.DebugOut(this.Request.Form);

        //Menus and the back button
        model.ShowBackButton = _showBackButton;
        model.BackPage = _backPage;
        model.NavItems = new List<NavItem>(this._handlerNavItems?.NavItems ?? Enumerable.Empty<NavItem>());

        //Tournament DTO.
        model.Tournament.Id = _sessionProxy?.TournamentId ?? 0;
        model.Tournament.Details.TeamSize = _sessionProxy?.TeamSize ?? 2;

        //Read teams from the displayed form
        List<Team> teams = _adaptorTeams.Parse(this.Request.Form, model.Tournament);

        //Validate teams before saving:
        //Team must have players
        //A player cannot appear more than once.
        TeamValidator teamValidator = new();
        var isvalidTeams = teamValidator.Check(teams, model.Tournament);
        //Warning or error
        if (isvalidTeams.Result != RetCodeTeamAction.Success)
        {
            Error = isvalidTeams.Result == RetCodeTeamAction.Error ? isvalidTeams?.Message ?? "" : "";
            model.Teams = teams;
            await LoadPage(model, false);
            return View("Index", model);
        }

        //Organization DTO
        Organization organization = new Organization() { Id = _sessionProxy?.OrganizationId ?? 1 };
        //Assign to repos

        //Assign references to the team dbrepo
        _dbRepoTeam.Organization = organization;
        _dbRepoTeam.TournamentId = model.Tournament.Id;

        //----------------------------------------
        //Sync between form and db
        //----------------------------------------

        //Get teams from db
        List<Team> teamsInDB = await _teamRepo.GetListAsync(model.Tournament.Id);

        SyncMaster<Team> syncMaster = new(teams, teamsInDB);
        //Add handlers to insert, update and delete teams
        syncMaster.Add += (sender, team) =>
        {
            //Add team to db
            if (team is not null) _dbRepoTeam.Set(team); ;
        };

        syncMaster.Update += (sender, team) =>
        {
            //Add team to db
            if (team is not null && team?.Dest is not null)
                _dbRepoTeam.Set(team.Dest);
        };

        syncMaster.Remove += (sender, team) =>
        {
            //Add team to db
            if (team is not null) _dbRepoTeam.Delete(team);
        };

        syncMaster.Run();
        //Get the next navigation item
        //Get the next navigation item
        var nextNavItem = NextPage("");
        if (nextNavItem is not null)
        {

            //Redirect to the next page
            return RedirectToAction(nextNavItem.Action, nextNavItem.Controller);
        }

        return View("Index", model);
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

        //Read teams from the displayed form
        model.Teams = _adaptorTeams.Parse(this.Request.Form, model.Tournament);

        var formAdaptor = new FormReaderPlayersList();
        var formTeams = formAdaptor.Parse(this.Request.Form, model.Tournament);

        var newTeam = formTeams.FirstOrDefault();
        if (newTeam is not null)
        {
            newTeam.Index = model.Teams.Count;
            //Add new team
            model.Teams.Add(newTeam);
        }

        //Add new team , don't need to validate and save to
        //db yet. Return;
        await LoadPage(model, false);
        return View("Index", model);
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

            //Select all players registered for the tournament
            //Get all players tournament id = 0

            Filter filterPlayer = new() { TournamentId = 0 };
            //Players registered for the tournament
            model.Players = (await _dbRepoPlayer.GetList(filterPlayer)) ?? new();
            //Set tournament properties for this page
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