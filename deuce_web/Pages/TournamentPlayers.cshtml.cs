using deuce;
using deuce_web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ZstdSharp.Unsafe;

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

    private readonly IFormReaderPlayers _adaptorTeams;

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
    DbRepoTournament dbRepoTournament, IFormReaderPlayers adaptorTeams, DbRepoMember dbRepoMember, DbRepoVenue dbRepoVenue,
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

        //Convert form values into a teams.
        Filter tourFilter = new() { TournamentId = _sessionProxy?.TournamentId ?? 0 } ;
        _tournamentDetail = (await _dbRepoTournamentDetail.GetList(tourFilter)).FirstOrDefault();

        if (_tournamentDetail is null)
        {
            Error = "Tournament detail not found";
            await LoadPage(false);
            return Page();
        }

        //Tournament DTO.
        Tournament tournament = new()
        {
            Id = _sessionProxy?.TournamentId ?? 0,  
            TeamSize = _tournamentDetail.TeamSize
        };

        //Read teams from the displayed form
        List<Team> teams = _adaptorTeams.Parse(HttpContext.Request.Form, tournament);

        //State
        //If the action is to add a team, then add it to _teams
        //and then show the same page ( Don't validate and save).
        string? action = this.HttpContext.Request.Form["action"];

        
        if (string.Compare(action ?? "", "add_team", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var formAdaptor = new FormReaderPlayersList();
            var formTeams = formAdaptor.Parse(HttpContext.Request.Form, tournament);
          
            var newTeam = formTeams.FirstOrDefault();
            if (newTeam is not null)
            {
                newTeam.Index = teams.Count;    
                //Add new team
                teams.Add(newTeam);
            }

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
        var isvalidTeams = teamValidator.Check(teams, tournament);
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
            _dbRepoTeam.Organization = new Organization() { Id = _sessionProxy?.OrganizationId ?? 1 };
            _dbRepoTeam.TournamentId = currentTourId;

            //----------------------------------------
            //Sync between form and db
            //----------------------------------------

            //Get teams from db
            List<Team> teamsInDB = await _teamRepo.GetListAsync(currentTourId);
            
            SyncMaster<Team> syncMaster = new(teams, teamsInDB);
            //Add handlers to insert, update and delete teams
            syncMaster.Add += (sender, team) =>
            {
                //Add team to db
                if(team is not null) _dbRepoTeam.Set(team);;
                 
            };


            syncMaster.Update +=  (sender, team) =>
            {
                //Add team to db
                  if (team is not null && team?.Dest is not null)
                        _dbRepoTeam.Set(team.Dest);
            };

            syncMaster.Remove +=  (sender, team) =>
            {
                //Add team to db
                if (team is not null) _dbRepoTeam.Delete(team);
            };

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

            //Select all players registered for the tournament
            //Get all players tournament id = 0

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
                //For GETS
                if (loadDB)
                {
                    //Load teams
                    Filter filterTeamPlayer = new Filter() { ClubId = organization.Id, TournamentId = currentTour.Id };
                    //Get the listing of players for the tournament.
                    List<RecordTeamPlayer> tourTeamPlayersRec = await _dbRepoRecordTeamPlayer.GetList(filterTeamPlayer);
                    //Create the team /player graph
                    TeamRepo teamRepo = new TeamRepo(tourTeamPlayersRec);
                    _teams = teamRepo.ExtractFromRecordTeamPlayer();

                    //Set player visibility
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


            //Remove players from the list that are already in a team
            foreach(Team team in _teams ?? new List<Team>())
            {
                foreach (Player player in team.Players)
                {
                    var foundPlayer = _players.Find(e=> e.Id == player.Id);  
                    if (foundPlayer is not null) foundPlayer.Team = team;
                }
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