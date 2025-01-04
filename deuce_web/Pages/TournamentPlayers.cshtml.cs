using System.Data.Common;
using System.Text.Json;
using deuce;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

public class TournamentPlayersPageModel : PageModel
{
    private readonly ILogger<TournamentPlayersPageModel> _log;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;
    public string? JSONPlayers { get; set; }

    public TournamentPlayersPageModel(ILogger<TournamentPlayersPageModel> log, IServiceProvider sp,
    IConfiguration config)
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
        
        try
        {
            await dbconn.OpenAsync();
            //Select all players in a club
            Organization organization = new Organization(){ Id = orgId };
            //For select elements
            
            //Use a DB repo
            DbRepoPlayer dbrpoPlayer = new DbRepoPlayer(dbconn, organization);
            var listOfPlayers = await dbrpoPlayer.GetList(new Filter() { ClubId = orgId});
            List<object> jsonobject = new();
            foreach(Player player in listOfPlayers)
            {
                var jsonObj = new {Name = player.ToString(), Id = player.Id};
                jsonobject.Add(jsonObj);
            }

            var someobj = new { Players = jsonobject.ToArray()};
            JSONPlayers = JsonSerializer.Serialize(someobj);


        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message);
        }

        return Page();

    }


}