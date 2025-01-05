using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Mvc.Filters;

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
        if (_showBackButton == "visible")  _backPage = _handlerNavItems.GetResourceAtIndex(selectedIdx-1);
        

        // string? uname = this.HttpContext.Session.GetString("user_name");
        // int? userId = this.HttpContext.Session.GetInt32("user_id");
        // _userName = uname ?? "";
        // _userId = userId ?? 0;

        // bool isInvalid = String.IsNullOrEmpty(uname) || !userId.HasValue;
        // _loggedIn = !isInvalid;
    }

    protected void SaveBackStack()
    {
        //"_handlerNavItems" is important!!
        if (_handlerNavItems is null) return;
        //Update navitems
        int selectedIdx = _handlerNavItems.GetSelectedIndex();
        if (selectedIdx>=0)
        {
            //If the current resource location 
            //changed, then update the list of
            //nav items.
            string? selResource = _handlerNavItems.GetResourceAtIndex(selectedIdx)??string.Empty;
            if (String.Compare(selResource, this.Request.PathBase, true) != 0) _handlerNavItems.UpdateResource(this.Request.PathBase);
        }
    }

}
