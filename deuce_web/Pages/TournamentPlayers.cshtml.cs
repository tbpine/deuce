using deuce;
using deuce_web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public class TournamentPlayersPageModel : BasePageModelWizard
{
    private readonly ILogger<TournamentPlayersPageModel> _log;
    private readonly DbRepoTournamentDetail _dbRepoTournamentDetail;
    private readonly DbRepoRecordTeamPlayer _dbRepoRecordTeamPlayer;
    private readonly DbRepoTeam _dbRepoTeam;
    private readonly DbRepoTournament _dbRepoTournament;
    private readonly DbRepoMember _dbRepoMember;
    private readonly DbRepoVenue _dbRepoVenue;
    private readonly DbRepoPlayer _dbRepoPlayer;
    private readonly TeamRepo _teamRepo;

    private readonly IAdaptorTeams _adaptorTeams;

    private List<Team>? _teams;

    public int NoTeams { get; set; }
    public int TeamSize { get; set; }
    public int EntryType { get; set; }
    public string Error { get; set; } = "";
    public List<List<SelectListItem>>? SelectMember { get; set; } = new();

    public List<Team>? Teams { get => _teams; }

    private List<Player>? _players;
    public List<Player>? Players { get=>_players;}

    private TournamentDetail? _tournamentDetail;

    public TournamentPlayersPageModel(ILogger<TournamentPlayersPageModel> log, IServiceProvider sp,
    IConfiguration config, IHandlerNavItems handlerNavItems,
    DbRepoTournamentDetail dbRepoTournamentDetail, DbRepoRecordTeamPlayer dbRepoRecordTeamPlayer, DbRepoTeam dbRepoTeam,
    DbRepoTournament dbRepoTournament, IAdaptorTeams adaptorTeams, DbRepoMember dbRepoMember, DbRepoVenue dbRepoVenue,
    TeamRepo teamRepo, DbRepoPlayer dbRepoPlayer) : base(handlerNavItems, sp, config)
    {
        _log = log;
        _teams = null;
        _dbRepoTournamentDetail = dbRepoTournamentDetail;
        _dbRepoRecordTeamPlayer = dbRepoRecordTeamPlayer;
        _dbRepoTeam = dbRepoTeam;
        _dbRepoTournament = dbRepoTournament;
        _adaptorTeams = adaptorTeams;
        _dbRepoMember = dbRepoMember;
        _dbRepoVenue = dbRepoVenue;
        _teamRepo = teamRepo;
        _dbRepoPlayer = dbRepoPlayer;
    }

    

    public async Task<IActionResult> OnGet()
    {
        try
        {
            await LoadPage();
        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message);
        }

        return Page();

    }

    public async Task<IActionResult> OnPost()
    {
        FormUtils.DebugOut(this.HttpContext.Request.Form);
        
        //Convert form values into a teams collection

        Organization thisOrg = new() { Id = _sessionProxy?.OrganizationId ?? 1 };

        //List<Team> teams = _adaptorTeams.Convert(HttpContext.Request.Form, thisOrg);

        //State
        //If the action is to add a team, then add it to _teams
        //and then show the same page ( Don't validate and save).
        string? action = this.HttpContext.Request.Form["action"];
        if (string.Compare(action ?? "", "add_team", StringComparison.OrdinalIgnoreCase) == 0)
        {

            //Load tournament details
            if (_tournamentDetail is null)
            {
                Filter filter = new() { ClubId = _sessionProxy?.OrganizationId ?? 0, TournamentId = _sessionProxy?.TournamentId ?? 0 };
                var listTourDetail = await _dbRepoTournamentDetail.GetList(filter);
                _tournamentDetail = listTourDetail.FirstOrDefault();
            }

            //Add new team
            Team newTeam = new Team() { Index = teams.Count() };

            for (int i = 0; i < (_tournamentDetail?.TeamSize ?? 0); i++)
                newTeam.AddPlayer(new Player() { Index = i });

            teams.Add(newTeam);
            //Add new team , don't need to validate and save to
            //db yet. Return;
            _teams = teams;
            await LoadPage(false);
            return Page();

        }

        //Validate teams before saving:
        //Team must have players
        //A player cannot appear more than once.
        TeamValidator teamValidator = new();
        var isvalidTeams = teamValidator.Check(teams);
        //Warning or error
        if (isvalidTeams.Result != RetCodeTeamAction.Success)
        {
            Error = isvalidTeams.Result == RetCodeTeamAction.Error ? isvalidTeams?.Message??"" : "";
            _teams = teams;
            await LoadPage(false);
            return Page();
        }


        //Assign to repos

        int currentTourId = _sessionProxy?.TournamentId ?? 0;

        if (currentTourId > 0)
        {
            //Assign references to the team dbrepo
            _dbRepoTeam.Organization = thisOrg;
            _dbRepoTeam.TournamentId = currentTourId;

            //----------------------------------------
            //Sync between form and db
            //----------------------------------------

            //Get teams from db
            List<Team> teamSrc = await _teamRepo.GetListAsync(currentTourId);
            
            SyncMaster<Team> syncMaster = new(teamSrc, teams);
            syncMaster.Run();

        }


        return NextPage("");
    }

    private async Task LoadPage(bool loadDB = true)
    {
        //Tournament format parameters
        int orgId = _sessionProxy?.OrganizationId ?? 1;
        EntryType = _sessionProxy?.EntryType ?? (int)deuce.EntryType.Team;
        int currentTourId = _sessionProxy?.TournamentId ?? 0;

        
        try
        {
            Organization organization = new Organization() { Id = orgId };
            Filter filter = new() { TournamentId = currentTourId , ClubId = organization.Id};
            //Need to know where the tournament is held, so members
            //can be listed
            var venue = (await _dbRepoVenue.GetList(filter)).FirstOrDefault();

            //Set country code
            filter.CountryCode = venue?.CountryCode??36;

            //Select all players registered for the tournament
            //Get all players now ToournamentId = 0

            Filter filterPlayer = new() { TournamentId = 0};
            _players = await _dbRepoPlayer.GetList(filterPlayer);
            var currentTour = (await _dbRepoTournament.GetList(filter)).FirstOrDefault();

            //Deflat saved teams
            if (currentTour is not null)
            {


                //Load tournament details
                if (_tournamentDetail is null)
                {
                    var listTourDetail = await _dbRepoTournamentDetail.GetList(filter);
                    _tournamentDetail = listTourDetail.FirstOrDefault();
                }

                if (loadDB)
                {
                    //Load teams
                    Filter filterTeamPlayer = new Filter() { ClubId = organization.Id, TournamentId = currentTour.Id };
                    //Get the listing of players for the tournament.
                    List<RecordTeamPlayer> tourTeamPlayersRec = await _dbRepoRecordTeamPlayer.GetList(filterTeamPlayer);
                    //Create the team /player graph
                    TeamRepo teamRepo = new TeamRepo(tourTeamPlayersRec);
                    _teams = teamRepo.ExtractFromRecordTeamPlayer();
                }

                //Don't use no of entries
                //Count the number of teams
                EntryType = currentTour.EntryType;
                NoTeams = _teams?.Count()??0;
                TeamSize = _tournamentDetail?.TeamSize ?? 1;

            }
            else
            {
                //Defauls;
                EntryType = (int)deuce.EntryType.Team;
                NoTeams = 2;
                TeamSize = 1;
            }


          

        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message);
        }
    }

    /// <summary>
    /// Reads an Excel file from the request and returns its content as a string.
    /// </summary>
    /// <returns>The content of the Excel file as a string, or an empty string if an error occurs.</returns>
    private async Task<string> ReadFileFromRequest()
    {
        // Check if a file was uploaded
        if (HttpContext.Request.Form.Files.Count > 0)
        {
            var file = HttpContext.Request.Form.Files[0];

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
                        }
                    }
                    catch (Exception ex)
                    {
                        Error = $"Error reading Excel file: {ex.Message}";
                        await LoadPage(false);
                        return "";
                    }
                }
                else
                {
                    Error = "Invalid file format. Please upload an Excel file.";
                    await LoadPage(false);
                    return "";
                }
            }
        }

        return "";
    }
   
}