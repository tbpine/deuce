using Microsoft.AspNetCore.Mvc.Rendering;
using deuce;


public class ViewModelFormatPlayer
{
    public string Title { get; set; } = "";

    public Format Format { get; set; } = new(1,1,1);

    public TournamentDetail TournamentDetail { get; set; } = new();

    public Tournament Tournament { get; set; } = new();

    public bool Validated { get; set; }

    public IEnumerable<SelectListItem> SelectGamesPerSet { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> SelectSets { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> SelectNoGames { get; set; } = new List<SelectListItem>();

}