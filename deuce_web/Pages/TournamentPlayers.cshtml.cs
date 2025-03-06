using System.Data.Common;
using System.Text.RegularExpressions;
using deuce;
using deuce_web.ext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualBasic;
using ZstdSharp.Unsafe;

public class TournamentPlayersPageModel : BasePageModelWizard
{
    private readonly ILogger<TournamentPlayersPageModel> _log;
    private readonly DbRepoPlayer _dbRepoPlayer;
    private readonly DbRepoTournamentDetail _dbRepoTournamentDetail;
    private readonly DbRepoRecordTeamPlayer _dbRepoRecordTeamPlayer;
    private readonly DbRepoTeam _dbRepoTeam;
    private readonly DbRepoTournament _dbRepoTournament;

    private List<Team>? _teams;
    public string? JSONPlayers { get; set; }

    public int NoTeams { get; set; }
    public int TeamSize { get; set; }
    public int EntryType { get; set; }
    public string Error { get; set; } = "";

    public string JSON { get; set; } = "";
    public List<List<SelectListItem>>? SelectPlayer { get; set; } = new();

    public List<Team>? Teams { get => _teams; }

    private TournamentDetail? _tournamentDetail;

    public TournamentPlayersPageModel(ILogger<TournamentPlayersPageModel> log, IServiceProvider sp,
    IConfiguration config, IHandlerNavItems handlerNavItems, DbRepoPlayer dbRepoPlayer,
    DbRepoTournamentDetail dbRepoTournamentDetail, DbRepoRecordTeamPlayer dbRepoRecordTeamPlayer, DbRepoTeam dbRepoTeam,
    DbRepoTournament dbRepoTournament) : base(handlerNavItems, sp, config)
    {
        _log = log;
        _teams = null;
        _dbRepoPlayer = dbRepoPlayer;
        _dbRepoTournamentDetail = dbRepoTournamentDetail;
        _dbRepoRecordTeamPlayer = dbRepoRecordTeamPlayer;
        _dbRepoTeam = dbRepoTeam;
        _dbRepoTournament = dbRepoTournament;
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

        List<Team> teams = new();
        //State
        Team? currentTeam = null;

        ///Transforms form values to teams
        foreach (var kp in HttpContext.Request.Form)
        {
            Console.WriteLine($"{kp.Key}={kp.Value}");
            //Path format 
            //team_(Index)_(Id)_player_(Index)_(TeamPlayerID)_new
            //Form value is id for existing players
            //and name for new players

            //Ignore the action value
            if (kp.Key == "action") continue;

            var matches = Regex.Match(kp.Key, @"team_(\d+)_(\d+)*(_player_)*(\d+)*_*(\d+)*(_new)*");

            if (matches.Success)
            {
                string teamIdx = matches.Groups[1].Value;
                string strTeamId = matches.Groups[2].Value;
                string playerIdx = matches.Groups[4].Value;
                string strPlayerTeamId = matches.Groups[5].Value;
                bool isNew = !string.IsNullOrEmpty(matches.Groups[6].Value);

                if (!string.IsNullOrEmpty(teamIdx) && string.IsNullOrEmpty(playerIdx) && !isNew)
                {
                    //Team form variable
                    string? strVal = kp.Value;
                    int idxTeam = int.TryParse(teamIdx, out idxTeam) ? idxTeam : 0;
                    int teamId = int.TryParse(strTeamId, out teamId) ? teamId : 0;
                    Team? team = new();
                    team.Id = teamId;
                    team.Label = string.IsNullOrEmpty(strVal) ? ("team_" + matches.Groups[1].Value) : strVal;
                    team.Index = idxTeam;

                    currentTeam = team;
                    teams.Add(currentTeam);

                }
                else if (!string.IsNullOrEmpty(teamIdx) && !string.IsNullOrEmpty(playerIdx) && !isNew)
                {
                    //Check if they selected a registered player
                    int playerId = this.GetFormInt(kp.Key);
                    int idxPlayer = int.TryParse(playerIdx, out idxPlayer) ? idxPlayer : 0;
                    int playerTeamId = int.TryParse(strPlayerTeamId, out playerTeamId) ? playerTeamId : 0;

                    Player player = new Player() { Id = playerId, Index = idxPlayer, TeamPlayerId = playerTeamId };

                    currentTeam?.AddPlayer(player);
                }
                else if (!string.IsNullOrEmpty(teamIdx) && !string.IsNullOrEmpty(playerIdx) && isNew &&
                !string.IsNullOrEmpty(kp.Value))
                {
                    string? strval = kp.Value;
                    //Split first and last names
                    string[] names = (strval ?? "").Split(" ");
                    string firstname = names.Length > 0 ? names[0].Trim() : "";
                    string lastname = names.Length > 1 ? names[1].Trim() : "";
                    int idxPlayer = int.TryParse(playerIdx, out idxPlayer) ? idxPlayer : 0;

                    Player player = new Player() { Id = -1, First = firstname, Last = lastname, Index = idxPlayer };
                    currentTeam?.AddPlayer(player);
                }
            }
        }
        //Validations

        //Check for teams with zero players
        List<Team> removeTeamList = new();
        foreach (Team team in teams)
            //Don't add teams with no players
            if (team.Players.Count() == 0) removeTeamList.Add(team);

        //Remove empty teams
        foreach (Team rmTeam in removeTeamList) teams.Remove(rmTeam);

        //If the action is to add a team
        //, then add it to _teams
        //and then show the same page.
        string? action = this.HttpContext.Request.Form["action"];
        if (string.Compare(action ?? "", "add_team", StringComparison.OrdinalIgnoreCase) == 0)
        {

            //Load tournament details
            if (_tournamentDetail is null)
            {
                Filter filter = new() { ClubId = _sessionProxy?.OrganizationId??0, TournamentId = _sessionProxy?.TournamentId??0 };
                var listTourDetail = await _dbRepoTournamentDetail.GetList(filter);
                _tournamentDetail = listTourDetail.FirstOrDefault();
            }

            //Add new team
            Team newTeam = new Team() { Index = teams.Count() };

            for (int i = 0; i < (_tournamentDetail?.TeamSize??0); i++)
                newTeam.AddPlayer(new Player() { Index = i });

            teams.Add(newTeam);


        }

        //Save teams to the database
        Organization thisOrg = new() { Id = _sessionProxy?.OrganizationId ?? 1 };

        //Assign to repos

        int currentTourId = _sessionProxy?.TournamentId ?? 0;

        if (currentTourId > 0)
        {
            //Assign references to the team dbrepo
            _dbRepoTeam.Organization = thisOrg;
            _dbRepoTeam.TournamentId = currentTourId;

            foreach (Team iterTeam in teams)
            {
                //Save players as well
                await _dbRepoTeam.SetAsync(iterTeam);
            }
        }


        if (string.Compare(action ?? "", "add_team", StringComparison.OrdinalIgnoreCase) == 0)
        {

            await LoadPage();

            return Page();
        }


        return NextPage("");
    }

    private async Task LoadPage()
    {
        //Tournament format parameters
        int orgId = _sessionProxy?.OrganizationId ?? 1;
        EntryType = _sessionProxy?.EntryType ?? (int)deuce.EntryType.Team;
        int currentTourId = _sessionProxy?.TournamentId ?? 0;
        try
        {

            //Select all players in a club
            Organization organization = new Organization() { Id = orgId };
            //Use a DB repo
            _dbRepoPlayer.Organization = organization;
            var orgPlayers = await _dbRepoPlayer.GetList(new Filter() { ClubId = orgId });
            var currentTour = (await _dbRepoTournament.GetList(new Filter() { TournamentId = currentTourId })).FirstOrDefault();

            //Deflat saved teams
            if (currentTour is not null)
            {


                //Load tournament details
                if (_tournamentDetail is null)
                {
                    Filter filter = new() { ClubId = organization.Id, TournamentId = currentTour.Id };
                    var listTourDetail = await _dbRepoTournamentDetail.GetList(filter);
                    _tournamentDetail = listTourDetail.FirstOrDefault();
                }

                //Load teams
                Filter filterTeamPlayer = new Filter() { ClubId = organization.Id, TournamentId = currentTour.Id };
                //Get the listing of players for the tournament.
                List<RecordTeamPlayer> tourTeamPlayersRec = await _dbRepoRecordTeamPlayer.GetList(filterTeamPlayer);
                //Create the team /player graph
                TeamRepo teamRepo = new TeamRepo(tourTeamPlayersRec);
                _teams = teamRepo.ExtractFromRecordTeamPlayer();
                
                //Don't use no of entries
                //Count the number of teams
                EntryType = currentTour.EntryType;
                NoTeams = _teams.Count();
                TeamSize = _tournamentDetail?.TeamSize ?? 1;

            }
            else
            {
                //Defauls;
                EntryType = (int)deuce.EntryType.Team;
                NoTeams = 2;
                TeamSize = 1;
            }

            //Create select of  players

            List<SelectListItem> playerList = new();
            //The empty player 
            playerList.Add(new SelectListItem("-- New Player --", ""));
            foreach (Player player in orgPlayers)
            {
                playerList.Add(new SelectListItem(player.ToString(), player.Id.ToString()));
            }
            //Default to a new player
            playerList[0].Selected = true;
            for (int i = 0; i < NoTeams * TeamSize; i++)
            {
                List<SelectListItem> copiedPlayerList = new(playerList);
                SelectPlayer?.Add(copiedPlayerList);
            }

            //Reconcile teams

            // for (int i = 0; i < NoTeams; i++)
            // {
            //     if (i >= _teams?.Count)
            //     {
            //         //Add a team
            //         Team newTeam = new Team();
            //         newTeam.Index = i;
            //         newTeam.Label = $"team_{i}";
            //         //Add players
            //         for (int j = 0; j < TeamSize; j++)
            //             newTeam.AddPlayer(new Player() { Index = j });
            //         _teams.Add(newTeam);
            //     }
            //     else
            //     {
            //         //Check if the stored teams have enough
            //         //players
            //         for (int j = 0; j < TeamSize; j++)
            //         {
            //             if (j >= _teams?[i].Players.Count())
            //                 _teams?[i].AddPlayer(new Player() { Index = j });
            //         }

            //     }
            // }

        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message);
        }
    }
}