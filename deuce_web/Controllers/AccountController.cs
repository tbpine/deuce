using Microsoft.AspNetCore.Mvc;
using deuce;
using deuce.ext;

public class AccountController : Controller
{
    private readonly DbRepoAccount _dbRepoAccount;
    private readonly DbRepoCountry _dbRepoCountry;
    private readonly DbRepoOrganization _dbRepoOrganization;
    private readonly ILogger<AccountController> _log;
    private readonly DbRepoSecurity _dbRepoSecurity;
    private readonly DbRepoMember _dbRepoMember;
    /// <summary>
    /// Inject dependencies here if needed.
    /// </summary>
    public AccountController(DbRepoAccount dbRepoAccount, ILogger<AccountController> log,
    DbRepoCountry dbRepoCountry, DbRepoOrganization dbRepoOrganization, DbRepoMember dbRepoMember,
    DbRepoSecurity dbRepoSecurity)
    {
        _log = log;
        _dbRepoAccount = dbRepoAccount;
        _dbRepoCountry = dbRepoCountry;
        _dbRepoOrganization = dbRepoOrganization;
        _dbRepoMember = dbRepoMember;
        _dbRepoSecurity = dbRepoSecurity;
    }
    public IActionResult Index()
    {
        return View();
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
                vmAcc.Member.PopulateNames(deuce.Utils.SplitNames(vmAcc.Name??""));
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
        //Get a list of security objects with this email and 
        //password
        //Check if the password is valid

        //Make the filter
        Filter filter = new() { Email = vmAcc.Account.Email, Password = vmAcc.Account.Password };
        var listOfSecurities = await _dbRepoSecurity.GetList(filter);
        bool passed = (listOfSecurities?.Count?? 0) > 0 && (listOfSecurities?.First().IsValid??false);

        //If passed security checks, then goto the "OrgIdx" page.
        //Else, show the index page again.
        if (passed)
            return Redirect("~/OrgIdx");
        else
            return RedirectToAction("Index");
    }

}