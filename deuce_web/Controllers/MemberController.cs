using Microsoft.AspNetCore.Mvc;
using deuce;
using Microsoft.AspNetCore.Mvc.Filters;
using DocumentFormat.OpenXml.Wordprocessing;

/// <summary>
/// Controller for member's area
/// </summary>
public class MemberController : Controller
{
    protected string _userName = string.Empty;
    protected int _userId = 0;

    protected bool _loggedIn = false;
    protected string? _showBackButton;
    protected string? _backPage;

    protected SessionProxy _sessionProxy;

    protected IServiceProvider _serviceProvider;
    protected IConfiguration _config;

    //Load tournament details
    protected ITournamentGateway _tourGateway;

    protected ISideMenuHandler _handlerNavItems;
    public IEnumerable<NavItem>? NavItems { get => _handlerNavItems?.NavItems; }

    //This works with the _Laywizard shared page.
    //That is the button next and back buttons
    public string? ShowBackButton { get => _showBackButton; set => _showBackButton = value; }
    public string? BackPage { get => _backPage; set => _backPage = value; }

    protected ViewModelMember _model;
    public MemberController(ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config,
    ITournamentGateway tgateway, SessionProxy sessionProxy)
    {
        _handlerNavItems = handlerNavItems;
        _serviceProvider = sp;
        _config = config;
        _tourGateway = tgateway;
        _sessionProxy = sessionProxy;
        _model = new ViewModelMember(_handlerNavItems);

    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        //Manage session
        if (_sessionProxy is not null) _sessionProxy.Session = context.HttpContext.Session;

        if (_handlerNavItems is null) return;

        _handlerNavItems.Set(this.HttpContext.Request.Path);

        //Find the back page
        int selectedIdx = _handlerNavItems.GetSelectedIndex();
        _showBackButton = selectedIdx > 0 ? "visible" : "invisible";
        //Set the URI of the last page in the list of
        //nav items.
        if (_showBackButton == "visible")
        {
            _backPage = _handlerNavItems.NavItems.ElementAt(selectedIdx - 1).Controller + "/" + _handlerNavItems.NavItems.ElementAt(selectedIdx - 1).Action;
        }

        //Fill _model properties with session values
        _model.Tournament.Id = _sessionProxy?.TournamentId ?? 0;
        _model.Organization.Id = _sessionProxy?.OrganizationId ?? 0;
        
        
    }

    protected IActionResult NextPage(string replacement)
    {
        if (_handlerNavItems is null) return View();

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
                return Redirect(HttpContext.Request.PathBase + "/OrgIdx");
            }
            else
                return Redirect(HttpContext.Request.PathBase + nextResource);

        }

        return View();

    }

}