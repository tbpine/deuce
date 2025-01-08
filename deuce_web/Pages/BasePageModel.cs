using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Mvc.Filters;
using Org.BouncyCastle.Asn1;
using Microsoft.AspNetCore.Mvc;

public class BasePageModel : PageModel
{
    protected string _userName = string.Empty;
    protected int _userId = 0;

    protected bool _loggedIn = false;
    protected string? _showBackButton;
    protected string? _backPage;
    

    protected IHandlerNavItems? _handlerNavItems;
    public IEnumerable<NavItem>? NavItems { get=>_handlerNavItems?.NavItems;}

    //This works with the _Laywizard shared page.
    //That is the button next and back buttons
    public string? ShowBackButton { get=>_showBackButton; set=>_showBackButton = value;}   
    public string? BackPage { get=>_backPage; set=>_backPage = value;}   

    public BasePageModel(IHandlerNavItems handlerNavItems)
    {
        _handlerNavItems = handlerNavItems;
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        base.OnPageHandlerExecuting(context);
        
        if (_handlerNavItems is null) return;

        _handlerNavItems.Set(this.HttpContext.Request.Path);

        //Find the back page
        int selectedIdx =  _handlerNavItems.GetSelectedIndex();
        _showBackButton  = selectedIdx > 0 ? "visible" : "invisible";
        //Set the URI of the last page in the list of
        //nav items.
        if (_showBackButton == "visible") 
        {
            _backPage = HttpContext.Request.PathBase + _handlerNavItems.GetResourceAtIndex(selectedIdx-1);
            if (_backPage.Contains("TournamentFormat"))
            {
                //case
                int entryType= this.HttpContext.Session.GetInt32("EntryType")??1;
                _backPage = HttpContext.Request.PathBase + (entryType == 1 ?  "/TournamentFormatTeams" : "/TournamentFormatPlayer");
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
        
        int selectedIdx = _handlerNavItems?.GetSelectedIndex()??-1;
        
        if (selectedIdx >=0 )
        {
            string nextResource = _handlerNavItems?.GetResourceAtIndex(selectedIdx +1)??"";
            return Redirect(HttpContext.Request.PathBase + nextResource);

        } 

        return Page();

    }
}
