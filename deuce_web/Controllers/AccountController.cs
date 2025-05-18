using Microsoft.AspNetCore.Mvc;
using deuce;
using deuce.ext;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Data.Common;

public class AccountController : Controller
{
    private readonly DbRepoAccount _dbRepoAccount;
    private readonly DbRepoCountry _dbRepoCountry;
    private readonly DbRepoOrganization _dbRepoOrganization;
    private readonly ILogger<AccountController> _log;
    private readonly DbRepoSecurity _dbRepoSecurity;
    private readonly DbRepoMember _dbRepoMember;
    private readonly DbConnection _dbconnection;

    private ISessionProxy _sessionProxy = new SessionProxy();
    /// <summary>
    /// Inject dependencies here if needed.
    /// </summary>
    public AccountController(DbRepoAccount dbRepoAccount, ILogger<AccountController> log,
    DbRepoCountry dbRepoCountry, DbRepoOrganization dbRepoOrganization, DbRepoMember dbRepoMember,
    DbRepoSecurity dbRepoSecurity, DbConnection dbconnection)
    {
        //Initialize the repositories
        _dbconnection = dbconnection;
        _dbRepoAccount = dbRepoAccount;
        _dbRepoCountry = dbRepoCountry;
        _dbRepoOrganization = dbRepoOrganization;
        _dbRepoMember = dbRepoMember;
        _dbRepoSecurity = dbRepoSecurity;
        _log = log;
    }

    /// <summary>
    /// Execute before any action method in this controller.
    /// </summary>
    /// <param name="context"></param>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        //Initialize the session proxy
        _sessionProxy = new SessionProxy(context.HttpContext.Session);

    }

    public IActionResult Index()
    {
        return View(new ViewModelAccount());
    }

    public async Task<IActionResult> SignUp()
    {
        ViewData["countries"] = (await _dbRepoCountry.GetList());
        return View(new ViewModelAccount());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(ViewModelAccount vmAcc)
    {
        try
        {
            //Save account details
            //If it's an organization, save
            //organization details.
            if (vmAcc.Account.Type == (int)(deuce.AccountType.Organizer))
            {
                //Activate organization
                vmAcc.Organization.Active = true;
                //Fill Name
                vmAcc.Organization.Name = vmAcc.Name ?? "";
                await _dbRepoOrganization.SetAsync(vmAcc.Organization);
                //Link to account
                vmAcc.Account.Organization = vmAcc.Organization.Id;
            }
            else if (vmAcc.Account.Type == (int)(deuce.AccountType.Player))
            {
                //Populate Member name
                vmAcc.Member.PopulateNames(deuce.Utils.SplitNames(vmAcc.Name ?? ""));
                //Set country
                vmAcc.Member.CountryCode = vmAcc.Account.Country;
                //Insert a row into the players table
                _dbRepoMember.Set(vmAcc.Member);
                //Link to the account
                vmAcc.Account.Member = vmAcc.Member.Id;
            }

            _dbRepoAccount.Set(vmAcc.Account);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error in Register method.");
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignIn(ViewModelAccount vmAcc)
    {
        ISecurityGateway secGateway = new SecurityGatewayDefault(_dbconnection);

        Account? acc = await secGateway.CheckPasswordAsync(vmAcc.Account.Email, vmAcc.Account.Password);
        if (acc is not null)
        {
            //Set session properties from the account object
            _sessionProxy.CurrentAccount = acc.Id;
            _sessionProxy.OrganizationId = acc.Organization;

            return RedirectToAction("Index", "Profile");
        }

        //Goto the member page
        return RedirectToAction("Index");

    }

}