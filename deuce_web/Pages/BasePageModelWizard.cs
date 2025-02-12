using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using deuce;
using System.Data.Common;

/// <summary>
/// Common class used during tournament creation
/// via the wizard.
/// </summary>
public class BasePageModelWizard : PageModel
{
    protected string _userName = string.Empty;
    protected int _userId = 0;

    protected bool _loggedIn = false;
    protected string? _showBackButton;
    protected string? _backPage;

    //Late binding variable create when the page
    //is accessed.
    protected SessionProxy? _sessionProxy;

    protected IServiceProvider _serviceProvider;
    protected IConfiguration _config;


    protected IHandlerNavItems? _handlerNavItems;
    public IEnumerable<NavItem>? NavItems { get => _handlerNavItems?.NavItems; }

    //This works with the _Laywizard shared page.
    //That is the button next and back buttons
    public string? ShowBackButton { get => _showBackButton; set => _showBackButton = value; }
    public string? BackPage { get => _backPage; set => _backPage = value; }

    public BasePageModelWizard(IHandlerNavItems handlerNavItems, IServiceProvider sp, IConfiguration config)
    {
        _handlerNavItems = handlerNavItems;
        _serviceProvider = sp;
        _config = config;
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        base.OnPageHandlerExecuting(context);
        //Manage session
         _sessionProxy = new SessionProxy(this.HttpContext.Session);

        if (_handlerNavItems is null) return;

        _handlerNavItems.Set(this.HttpContext.Request.Path);

        //Find the back page
        int selectedIdx = _handlerNavItems.GetSelectedIndex();
        _showBackButton = selectedIdx > 0 ? "visible" : "invisible";
        //Set the URI of the last page in the list of
        //nav items.
        if (_showBackButton == "visible")
        {
            _backPage = HttpContext.Request.PathBase + _handlerNavItems.GetResourceAtIndex(selectedIdx - 1);
            if (_backPage.Contains("TournamentFormat"))
            {
                //case
                int entryType = _sessionProxy?.EntryType??1;
                _backPage = HttpContext.Request.PathBase + (entryType == 1 ? "/TournamentFormatTeams" : "/TournamentFormatPlayer");
            }
        }


        // string? uname = this.HttpContext.Session.GetString("user_name");
        // int? userId = this.HttpContext.Session.GetInt32("user_id");
        // _userName = uname ?? "";
        // _userId = userId ?? 0;

        // bool isInvalid = String.IsNullOrEmpty(uname) || !userId.HasValue;
        // _loggedIn = !isInvalid;
    }

    protected IActionResult NextPage(string replacement)
    {
        if (_handlerNavItems is null) return Page();

        if (!String.IsNullOrEmpty(replacement))
        {
            return Redirect(HttpContext.Request.PathBase + replacement);

        }

        int selectedIdx = _handlerNavItems?.GetSelectedIndex() ?? -1;

        if (selectedIdx >= 0)
        {
            string nextResource = _handlerNavItems?.GetResourceAtIndex(selectedIdx + 1) ?? "";
            if (string.IsNullOrEmpty(nextResource))
            {
                //End of wizard
                //Redirct to finishing point
                //Schedule tournament
                
                return Redirect(HttpContext.Request.PathBase + "/OrgIdx");
            }
            else
                return Redirect(HttpContext.Request.PathBase + nextResource);

        }

        return Page();

    }

    protected async Task<Tournament?> GetCurrentTournament(DbConnection? dbconn)
    {
        //Check if there's a tournament saved
        int tourId = _sessionProxy?.TournamentId ?? 0;
        if (tourId < 1) return null;

        //On demand connection
        bool closeConnection = false;

        if (dbconn is null)
        {
            var scope = _serviceProvider.CreateScope();
            dbconn = scope.ServiceProvider.GetRequiredService<DbConnection>();
            dbconn.ConnectionString = _config.GetConnectionString("deuce_local");
            await dbconn.OpenAsync();
            closeConnection = true;
        }


        Organization thisOrg = new() { Id = _sessionProxy?.OrganizationId??1 , Name=""};

        //Load a the current tournament from the database
        //Create a scoped db connection.
        //Use a DBRepo to build the object
        DbRepoTournament dbRepoTour = new DbRepoTournament(dbconn, thisOrg);
        //Select the tournment.Returns in the first element
        //Create filter
        Filter tourFilter = new Filter() { TournamentId = tourId };
        List<Tournament> listOfTour = await dbRepoTour.GetList(tourFilter);
        //Close if it was created.
        if (closeConnection ) await dbconn.CloseAsync();

        return listOfTour.FirstOrDefault();


    }

    protected async Task SetCurrentTournament(Tournament obj, IServiceProvider serviceProvider, IConfiguration cfg, Organization organization)
    {
        //Create a scoped db connection.
        using (var scope = serviceProvider.CreateScope())
        {
            var dbconn = scope.ServiceProvider.GetService<DbConnection>();
            if (dbconn is null) return;
            //open the connection
            dbconn.ConnectionString = cfg.GetConnectionString("deuce_local");
            await dbconn.OpenAsync();
            //Use a DBRepo to build the object
            DbRepoTournament dbRepoTour = new DbRepoTournament(dbconn, organization);

            await dbRepoTour.SetAsync(obj);
            await dbconn.CloseAsync();

        }
    }

}
