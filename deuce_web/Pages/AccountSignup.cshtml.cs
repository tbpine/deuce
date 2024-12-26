using Microsoft.AspNetCore.Mvc.RazorPages;

/// <summary>
/// 
/// </summary>
public class AccountSignUpPageModel : PageModel
{
    private readonly ILogger<AccountSignUpPageModel> _log;
    public AccountSignUpPageModel(ILogger<AccountSignUpPageModel> log)
    {
        _log = log;
    }
}