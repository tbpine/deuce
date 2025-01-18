using System.Data.Common;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;
using deuce;
using deuce_web.ext;
using iText.Layout.Properties.Grid;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mysqlx;

public class TournamentPlayersPageModel : BasePageModel
{
    private readonly ILogger<TournamentPlayersPageModel> _log;
    private List<Team>? _teams;
    public string? JSONPlayers { get; set; }

    public int NoTeams { get; set; }
    public int TeamSize { get; set; }
    public int EntryType { get; set; }
    public string Error { get; set; } = "";

    public string JSON { get; set; } = "";
    public List<List<SelectListItem>>? SelectPlayer { get; set; } = new();

    public List<Team>? Teams { get => _teams; }

    public TournamentPlayersPageModel(ILogger<TournamentPlayersPageModel> log, IServiceProvider sp,
    IConfiguration config, IHandlerNavItems handlerNavItems) : base(handlerNavItems, sp, config)
    {
        _log = log;
        _teams = null;
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

        foreach (var kp in HttpContext.Request.Form)
        {
            Console.WriteLine($"{kp.Key}={kp.Value}");
            //Path format 
            //team_(Index)_(Id)_player_(Index)_(Id)_new

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
                    int playerTeamId =  int.TryParse(strPlayerTeamId, out playerTeamId) ? playerTeamId : 0;
                    
                    if (playerId > 0)
                    {
                        Player player = new Player() { Id = playerId, Index = idxPlayer, TeamPlayerId = playerTeamId };

                        currentTeam?.AddPlayer(player);
                    }
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

                    Player player = new Player() { First = firstname, Last = lastname, Index = idxPlayer };
                    currentTeam?.AddPlayer(player);
                }
            }
        }

        
        foreach (Team team in teams)
        {
            bool allPlayersValid = team.Players.All(e => String.IsNullOrEmpty(e.First) && String.IsNullOrEmpty(e.Last));
            if (!allPlayersValid)
            {
                Error = "Missing Player names";
                //Return the page
                await LoadPage();
                return Page();
            }

        }

        //Save teams to the database
        var scope = _serviceProvider.CreateScope();
        var dbconn = scope.ServiceProvider.GetRequiredService<DbConnection>();
        if (dbconn is not null)
        {
            dbconn.ConnectionString = _config.GetConnectionString("deuce_local");
            await dbconn.OpenAsync();
            Organization thisOrg = new() { Id = _sessionProxy?.OrganizationId ?? 1 };
            int currentTourId = _sessionProxy?.TournamentId ?? 0;

            if (currentTourId > 0)
            {

                foreach (Team iterTeam in teams)
                {
                    //Saves player as well
                    DbRepoTeam dbRepoTeam = new DbRepoTeam(dbconn, thisOrg, currentTourId, true);
                    await dbRepoTeam.Set(iterTeam);
                }
            }


        }

        return NextPage("");
    }

    private async Task LoadPage()
    {
        //Tournament format parameters
        int orgId = _sessionProxy?.OrganizationId ?? 1;
        EntryType = _sessionProxy?.EntryType ?? 1;

        var scope = _serviceProvider.CreateScope();
        var dbconn = scope.ServiceProvider.GetRequiredService<DbConnection>();
        dbconn.ConnectionString = _config.GetConnectionString("deuce_local");



        try
        {
            await dbconn.OpenAsync();

            //Select all players in a club
            Organization organization = new Organization() { Id = orgId };
            //Use a DB repo
            DbRepoPlayer dbrpoPlayer = new DbRepoPlayer(dbconn, organization);
            var orgPlayers = await dbrpoPlayer.GetList(new Filter() { ClubId = orgId });

            //Deflat saved teams
            var currentTour = await GetCurrentTournament(dbconn);
            if (currentTour is not null)
            {

                //Load tournament details
                DbRepoTournamentDetail dbRepoTourDetail = new(dbconn, organization);
                Filter filter = new() { ClubId = organization.Id, TournamentId = currentTour.Id };
                var listTourDetail = await dbRepoTourDetail.GetList(filter);
                var tourDetail = listTourDetail.FirstOrDefault();

                EntryType = currentTour.EntryType;
                NoTeams = tourDetail?.NoEntries ?? 2;
                TeamSize = tourDetail?.TeamSize ?? 1;

                //Load teams
                DbRepoRecordTeamPlayer dbRepoRecordTP = new DbRepoRecordTeamPlayer(dbconn);
                Filter filterTeamPlayer = new Filter() { ClubId = organization.Id, TournamentId = currentTour.Id };
                //Get the listing of players for the tournament.
                List<RecordTeamPlayer> tourTeamPlayersRec = await dbRepoRecordTP.GetList(filterTeamPlayer);
                //Create the team /player graph
                TeamRepo teamRepo = new TeamRepo();
                _teams = teamRepo.ExtractFromRecordTeamPlayer(tourTeamPlayersRec, orgPlayers, organization);
            }
            else
            {
                //Defauls;
                EntryType = 1;
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

            for (int i = 0; i < NoTeams; i++)
            {
                if (i >= _teams?.Count)
                {
                    //Add a team
                    Team newTeam = new Team();
                    newTeam.Index = i;
                    newTeam.Label = $"team_{i}";
                    //Add players
                    for (int j = 0; j < TeamSize; j++)
                        newTeam.AddPlayer(new Player() { Index = j });
                    _teams.Add(newTeam);
                }
                else
                {
                    //Check if the stored teams have enough
                    //players
                    for (int j = 0; j < TeamSize; j++)
                    {
                        if (j >= _teams?[i].Players.Count())
                            _teams?[i].AddPlayer(new Player() { Index = j });
                    }
                        
                }
            }

        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message);
        }
    }
}