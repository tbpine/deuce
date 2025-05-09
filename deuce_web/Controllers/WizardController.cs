using Microsoft.AspNetCore.Mvc;
using deuce;
using Microsoft.AspNetCore.Mvc.Filters;

public abstract class WizardController : Controller
{
    protected string _userName = string.Empty;
    protected int _userId = 0;

    protected bool _loggedIn = false;
    protected string? _showBackButton;
    protected string? _backPage;

    //Late binding variable create when the page
    //is accessed.
    protected SessionProxy _sessionProxy = new();

    protected IServiceProvider _serviceProvider;
    protected IConfiguration _config;

    protected IHandlerNavItems? _handlerNavItems;
    public IEnumerable<NavItem>? NavItems { get => _handlerNavItems?.NavItems; }

    //This works with the _Laywizard shared page.
    //That is the button next and back buttons
    public string? ShowBackButton { get => _showBackButton; set => _showBackButton = value; }
    public string? BackPage { get => _backPage; set => _backPage = value; }

    public WizardController(IHandlerNavItems handlerNavItems, IServiceProvider sp, IConfiguration config)
    {
        _handlerNavItems = handlerNavItems;
        _serviceProvider = sp;
        _config = config;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);
        //Manage session
        _sessionProxy.Session = this.HttpContext.Session;

        if (_handlerNavItems is null) return;
                // Get the controller name
        string? controllerName = (string?)HttpContext.Request.RouteValues["controller"];
        
        // Get the action name
        string? actionName = (string?)HttpContext.Request.RouteValues["action"];

        _handlerNavItems.SetControllerAction(controllerName??"", actionName??"");

        //Find the back page
        int selectedIdx = _handlerNavItems.GetSelectedIndex();
        _showBackButton = selectedIdx > 0 ? "visible" : "invisible";
        //Set the URI of the last page in the list of
        //nav items.
        if (_showBackButton == "visible")
        {
            var prevNavItem = _handlerNavItems.NavItems.ElementAt(selectedIdx - 1);
            _backPage = prevNavItem.Controller + "/" + prevNavItem.Action;    
            
            if (prevNavItem.Controller.Contains("TFormat"))
            {
                //case
                int entryType = _sessionProxy?.EntryType ?? (int)EntryType.Team;
                _backPage = (entryType == (int)EntryType.Team ? "TFormatTeams" : "TFormatPlayer")
                + "/" + "Index";
                
            }
        }


        // string? uname = this.HttpContext.Session.GetString("user_name");
        // int? userId = this.HttpContext.Session.GetInt32("user_id");
        // _userName = uname ?? "";
        // _userId = userId ?? 0;

        // bool isInvalid = String.IsNullOrEmpty(uname) || !userId.HasValue;
        // _loggedIn = !isInvalid;
    }   

    protected NavItem? NextPage(string replacement)
    {
        if (_handlerNavItems is null) return null;

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

}
