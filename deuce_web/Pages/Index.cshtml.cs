using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using deuce;

namespace deuce_web.Pages;


public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IServiceProvider _sc;

    public List<Interval> Intervals { get; set; } = new();

    public IndexModel(ILogger<IndexModel> logger, IServiceProvider sc)
    {
        _logger = logger;
        _sc = sc;
    }

    public IActionResult OnGet()
    {
        return Page();
    }
}
