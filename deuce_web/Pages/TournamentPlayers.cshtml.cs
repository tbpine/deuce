using System.Data.Common;
using System.Security.Policy;
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
    private readonly DbRepoMember _dbRepoMember;
    private readonly DbRepoVenue _dbRepoVenue;

    private readonly IAdaptorTeams _adaptorTeams;

    private List<Team>? _teams;
    public string? JSONPlayers { get; set; }

    public int NoTeams { get; set; }
    public int TeamSize { get; set; }
    public int EntryType { get; set; }
    public string Error { get; set; } = "";

    public string JSON { get; set; } = "";
    public List<List<SelectListItem>>? SelectMember { get; set; } = new();

    public List<Team>? Teams { get => _teams; }

    private TournamentDetail? _tournamentDetail;

    public TournamentPlayersPageModel(ILogger<TournamentPlayersPageModel> log, IServiceProvider sp,
    IConfiguration config, IHandlerNavItems handlerNavItems, DbRepoPlayer dbRepoPlayer,
    DbRepoTournamentDetail dbRepoTournamentDetail, DbRepoRecordTeamPlayer dbRepoRecordTeamPlayer, DbRepoTeam dbRepoTeam,
    DbRepoTournament dbRepoTournament, IAdaptorTeams adaptorTeams, DbRepoMember dbRepoMember, DbRepoVenue dbRepoVenue) : base(handlerNavItems, sp, config)
    {
        _log = log;
        _teams = null;
        _dbRepoPlayer = dbRepoPlayer;
        _dbRepoTournamentDetail = dbRepoTournamentDetail;
        _dbRepoRecordTeamPlayer = dbRepoRecordTeamPlayer;
        _dbRepoTeam = dbRepoTeam;
        _dbRepoTournament = dbRepoTournament;
        _adaptorTeams = adaptorTeams;
        _dbRepoMember = dbRepoMember;
        _dbRepoVenue = dbRepoVenue;
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
        //Convert form values into a teams collection

        Organization thisOrg = new() { Id = _sessionProxy?.OrganizationId ?? 1 };

        List<Team> teams = _adaptorTeams.Convert(HttpContext.Request.Form, thisOrg);

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

            //Save teams to the database
            foreach (Team iterTeam in teams)
            {

                //Save players as well
                await _dbRepoTeam.SetAsync(iterTeam);
            }
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

            //Select all players in a country
            //Use a DB repo
            var countryMembers = await _dbRepoMember.GetList(filter);
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

            //Create select of  players

            List<SelectListItem> memberList = new();
            //The empty player 
            memberList.Add(new SelectListItem("-- New Player --", ""));
            foreach (Member member in countryMembers)
            {
                memberList.Add(new SelectListItem(member.ToString(), member.Id.ToString()));
            }
            //Default to a new player
            memberList[0].Selected = true;
            for (int i = 0; i < NoTeams * TeamSize; i++)
            {
                List<SelectListItem> copiedMemberList = new(memberList);
                SelectMember?.Add(copiedMemberList);
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