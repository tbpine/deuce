using System.Data.Common;
using System.Text.Json;
using deuce;
using deuce_web.ext;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

public class TournamentPlayersPageModel : BasePageModel
{
    private readonly ILogger<TournamentPlayersPageModel> _log;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;
    public string? JSONPlayers { get; set; }

    public int NoTeams { get; set; }
    public int TeamSize { get; set; }
    public int EntryType { get; set; }
    public List<SelectListItem>? SelectPlayer { get; set;} 

    public TournamentPlayersPageModel(ILogger<TournamentPlayersPageModel> log, IServiceProvider sp,
    IConfiguration config,  IHandlerNavItems handlerNavItems) : base(handlerNavItems)
    {
        _log = log;
        _serviceProvider = sp;
        _config = config;

    }

    public async Task<IActionResult> OnGet()
    {
        var scope = _serviceProvider.CreateScope();
        var dbconn = scope.ServiceProvider.GetRequiredService<DbConnection>();
        dbconn.ConnectionString = _config.GetConnectionString("deuce_local");
        int orgId = this.HttpContext.Session.GetInt32("organization_id")??1;

        EntryType = this.HttpContext.Session.GetInt32("EntryType")??1;
        NoTeams = EntryType == 1 ? this.HttpContext.Session.GetInt32("NoTeams")??2:
            this.HttpContext.Session.GetInt32("NoPlayers")??2;
        TeamSize = EntryType == 1 ? this.HttpContext.Session.GetInt32("TeamSize")??2: 1;
                
        try
        {
            await dbconn.OpenAsync();
            //Select all players in a club
            Organization organization = new Organization(){ Id = orgId };
            //For select elements
            
            //Use a DB repo
            DbRepoPlayer dbrpoPlayer = new DbRepoPlayer(dbconn, organization);
            var listOfPlayers = await dbrpoPlayer.GetList(new Filter() { ClubId = orgId});
            SelectPlayer = new();
            //The empty player 
            SelectPlayer.Add(new SelectListItem("",""));
            foreach(Player player in listOfPlayers)
            {
                SelectPlayer.Add(new SelectListItem(player.ToString(), player.Id.ToString()));
            }


        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message);
        }

        return Page();

    }

    public async Task<IActionResult> OnPost()
    {
        this.SaveToSession();
        foreach(var kp in HttpContext.Request.Form)
        {
            Console.WriteLine($"{kp.Key}={kp.Value}");
        }

        return NextPage("");
    }

}