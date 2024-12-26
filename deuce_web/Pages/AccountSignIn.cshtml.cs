using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class AccountSignInPageModel : PageModel
{
    private readonly ILogger<AccountSignInPageModel> _logger;
    
    /// <summary>
    /// Construct with dependencies
    /// </summary>
    /// <param name="logger"></param>
    public AccountSignInPageModel(ILogger<AccountSignInPageModel> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await Task.CompletedTask;
        return Page();
    }
} 