using Microsoft.AspNetCore.Mvc;
using deuce;
using Microsoft.AspNetCore.Mvc.Filters;
using DocumentFormat.OpenXml.EMMA;

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

    protected ViewModelTournamentWizard _model = new();

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

        _handlerNavItems.SetControllerAction(controllerName ?? "");

        //Find the back page
        int selectedIdx = _handlerNavItems.GetSelectedIndex();
        _showBackButton = selectedIdx > 0 ? "visible" : "invisible";
        //Set the URI of the last page in the list of
        //nav items.
        if (_showBackButton == "visible")
        {
            var prevNavItem = _handlerNavItems.NavItems.ElementAt(selectedIdx - 1);
            _backPage = prevNavItem.Controller + "/" + prevNavItem.Action;

            if (prevNavItem.Controller.Contains("TDetail"))
            {
                _model.RouteData = (_sessionProxy?.TournamentId??0).ToString();
            }   
        }
        //Menus and back navigation
        _model.NavItems = new List<NavItem>(_handlerNavItems.NavItems);
        _model.ShowBackButton = _showBackButton;
        _model.BackPage = _backPage;
        //Fill model with saved session values
        _model.Tournament.Id = _sessionProxy?.TournamentId ?? 0;
        _model.Tournament.Organization = new Organization() { Id = _sessionProxy?.OrganizationId??0, Name = "" };
        _model.Tournament.EntryType =  _sessionProxy?.EntryType ?? (int)EntryType.Team;
        _model.Tournament.Details.TeamSize = _sessionProxy?.TeamSize ?? 1;
        
    }

    protected NavItem? NextPage(string replacement)
    {
        if (_handlerNavItems is null) return null;

        int selectedIdx = _handlerNavItems?.GetSelectedIndex() ?? -1;
        //If the replacement is a non empty string, then split
        //into controller and action.
        if (!string.IsNullOrEmpty(replacement))
        {
            string[] parts = replacement.Split('/');
            if (parts.Length == 2)
            {
                //Find the index of the item in the list
                var selectedNavItem = _handlerNavItems?.NavItems.ElementAt(selectedIdx);
                if (selectedNavItem is not null)
                {
                    selectedNavItem.Action = parts[1];
                    selectedNavItem.Controller = parts[0];
                }
                
            }
        }

        return selectedIdx >= 0 && selectedIdx < _handlerNavItems?.NavItems.Count() - 1 ? _handlerNavItems?.NavItems.ElementAt(selectedIdx + 1) : null;

    }

}
