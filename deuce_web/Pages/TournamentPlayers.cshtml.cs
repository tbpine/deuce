using System.Data.Common;
using System.Text.RegularExpressions;
using deuce;
using deuce_web.ext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mysqlx;

public class TournamentPlayersPageModel : BasePageModel
{
    private readonly ILogger<TournamentPlayersPageModel> _log;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;
    private List<Team>? _teams;
    public string? JSONPlayers { get; set; }

    public int NoTeams { get; set; }
    public int TeamSize { get; set; }
    public int EntryType { get; set; }
    public string Error { get; set; } = "";
    public List<List<SelectListItem>>? SelectPlayer { get; set; } = new();

    public List<Team>? Teams { get => _teams; }

    public TournamentPlayersPageModel(ILogger<TournamentPlayersPageModel> log, IServiceProvider sp,
    IConfiguration config, IHandlerNavItems handlerNavItems) : base(handlerNavItems)
    {
        _log = log;
        _serviceProvider = sp;
        _config = config;
        _teams = null;
    }

    public async Task<IActionResult> OnGet()
    {
        await LoadPage();

        return Page();

    }

    public async Task<IActionResult> OnPost()
    {
        this.SaveToSession();
        List<Team> teams = new();
        //State
        Team? currentTeam = null;

        foreach (var kp in HttpContext.Request.Form)
        {
            Console.WriteLine($"{kp.Key}={kp.Value}");

            var matches = Regex.Match(kp.Key, @"team_(\d+)(_player_)*(\d+)*(_new)*");

            if (matches.Success)
            {
                string teamIdx = matches.Groups[1].Value;
                string playerIdx = matches.Groups[3].Value;
                bool isNew = !string.IsNullOrEmpty(matches.Groups[4].Value);

                if (!string.IsNullOrEmpty(teamIdx) && string.IsNullOrEmpty(playerIdx) && !isNew)
                {
                    //Team form variable
                    string? strVal = kp.Value;
                    Team team = new() { Label = string.IsNullOrEmpty(strVal) ? ("team_" + matches.Groups[1].Value) : strVal };
                    currentTeam = team;
                    teams.Add(currentTeam);

                }
                else if (!string.IsNullOrEmpty(teamIdx) && !string.IsNullOrEmpty(playerIdx) && !isNew)
                {
                    //Check if they selected a registered player
                    int playerId = this.GetFormInt(kp.Key);
                    if (playerId > 0)
                    {
                        Player player = new Player() { Id = this.GetFormInt(kp.Key) };
                        currentTeam?.AddPlayer(player);
                    }
                }
                else if (!string.IsNullOrEmpty(teamIdx) && !string.IsNullOrEmpty(playerIdx) && isNew)
                {
                    string? strval = kp.Value;
                    //Split first and last names
                    string[] names = (strval ?? "").Split(" ");
                    string firstname = names.Length > 0 ? names[0] : "";
                    string lastname = names.Length > 1 ? names[1] : "";

                    Player player = new Player() { First = firstname, Last = lastname };
                    currentTeam?.AddPlayer(player);
                }
            }
        }

        //Save teams to the database
        var scope = _serviceProvider.CreateScope();
        var dbconn = scope.ServiceProvider.GetRequiredService<DbConnection>();
        if (dbconn is not null)
        {
            dbconn.ConnectionString = _config.GetConnectionString("deuce_local");
            await dbconn.OpenAsync();
            Organization thisOrg = new() { Id = 1, Name = "testing" };
            int currentTourId = HttpContext.Session.GetInt32("CurrentTournament") ?? 0;

            foreach (Team iterTeam in teams)
            {
                //Saves player as well
                DbRepoTeam dbRepoTeam = new DbRepoTeam(dbconn, thisOrg, currentTourId, true);
                await dbRepoTeam.Set(iterTeam);
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

        return NextPage("");
    }

    private async Task LoadPage()
    {
        //Tournament format parameters
        int orgId = this.HttpContext.Session.GetInt32("organization_id") ?? 1;
        EntryType = this.HttpContext.Session.GetInt32("EntryType") ?? 1;
        NoTeams = EntryType == 1 ? this.HttpContext.Session.GetInt32("NoTeams") ?? 2 :
            this.HttpContext.Session.GetInt32("NoPlayers") ?? 2;
        TeamSize = EntryType == 1 ? this.HttpContext.Session.GetInt32("TeamSize") ?? 2 : 1;

        var scope = _serviceProvider.CreateScope();
        var dbconn = scope.ServiceProvider.GetRequiredService<DbConnection>();
        dbconn.ConnectionString = _config.GetConnectionString("deuce_local");

        try
        {
            await dbconn.OpenAsync();
            //Select all players in a club
            Organization organization = new Organization() { Id = orgId };
            //For select elements

            //Use a DB repo
            DbRepoPlayer dbrpoPlayer = new DbRepoPlayer(dbconn, organization);
            var orgPlayers = await dbrpoPlayer.GetList(new Filter() { ClubId = orgId });
            List<SelectListItem> playerList = new();
            //The empty player 
            playerList.Add(new SelectListItem("-- New Player --", ""));
            foreach (Player player in orgPlayers)
            {
                playerList.Add(new SelectListItem(player.ToString(), player.Id.ToString()));
            }
            //Default to a new player
            playerList[0].Selected = true;
            for(int i = 0 ; i < NoTeams*TeamSize; i++)
            {
                List<SelectListItem> copiedPlayerList = new(playerList);
                SelectPlayer?.Add(copiedPlayerList);
            }

            //Deflat saved teams
            int currentTourId = HttpContext.Session.GetInt32("CurrentTournament")??0;
            //Load teams
            DbRepoRecordTeamPlayer dbRepoRecordTP = new DbRepoRecordTeamPlayer(dbconn);
            Filter filterTeamPlayer = new Filter() { ClubId = organization.Id, TournamentId = currentTourId};
            //Get the listing of players for the tournament.
            List<RecordTeamPlayer> tourTeamPlayersRec = await dbRepoRecordTP.GetList(filterTeamPlayer);
            //Create the team /player graph
            TeamRepo teamRepo = new TeamRepo();
            _teams =  teamRepo.ExtractFromRecordTeamPlayer(tourTeamPlayersRec,orgPlayers, organization);
            

        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message);
        }
    }
}