using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

/// <summary>
/// 
/// </summary>
public class TournamentDetailPageModel : PageModel
{
    private readonly ILogger<TournamentDetailPageModel> _log;
    public TournamentDetailPageModel(ILogger<TournamentDetailPageModel> log)
    {
        _log = log;
    }

    public IActionResult OnPost()
    {
        string?  strSport= this.Request.Form["type"];
        string?  strTournamentType= this.Request.Form["category"];
        int sportId = int.Parse(strSport??"");
        int tournamentType = int.Parse(strTournamentType??"");

        this.HttpContext.Session.SetInt32("sport", sportId);
        this.HttpContext.Session.SetInt32("tournament_type", tournamentType);

        
        
        return Redirect("/TournamentFormat");
    }

}