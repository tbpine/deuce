using Microsoft.AspNetCore.Mvc;
using deuce;

public class AccountController : Controller
{
    private readonly DbRepoAccount _dbRepoAccount;
    private readonly DbRepoCountry _dbRepoCountry;
    private readonly ILogger<AccountController> _log;
    /// <summary>
    /// Inject dependencies here if needed.
    /// </summary>
    public AccountController(DbRepoAccount dbRepoAccount, ILogger<AccountController> log,
    DbRepoCountry dbRepoCountry)
    {
        _log = log;
        _dbRepoAccount = dbRepoAccount;
        _dbRepoCountry = dbRepoCountry;
    }
    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> SignUp()
    {
        ViewData["countries"] = (await _dbRepoCountry.GetList());
        return View();        
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(Account account)
    {
        try
        { 
            _dbRepoAccount.Set(account);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error in Register method.");
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SignIn(Account account)
    {
        return View();
    }

}