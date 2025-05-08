
/// <summary>
/// Handles menu navigation in the account section
/// 
/// </summary>
public class AccSideMenuHandler : ISideMenuHandler
{
    private List<NavItem> _navItems = new();

    public IEnumerable<NavItem> NavItems { get=> _navItems; }

    public AccSideMenuHandler()
    {
        _navItems.Add(new NavItem("My profile", "/OrgIdx", false, true, "fi-user", "Member", "Index"));
        _navItems.Add(new NavItem("Tournaments", "/Tournaments", false, true, "fi-layers", "Tournament", "Index"));
        _navItems.Add(new NavItem("Payment details", "/PaymentDetails", false, true, "fi-credit-card", "Payment", "Index"));
        _navItems.Add(new NavItem("Account settings", "/AccSettings", false, true, "fi-settings", "Account", "Settings"));
        _navItems.Add(new NavItem("Help centre", "/HelpCentre", false, true, "fi-help-circle", "HelpCenter", "Index"));

    }

 /// <summary>
    /// Set the active menu selection
    /// </summary>
    /// <param name="resource">Resource path</param>
    public void Set(string resource)
    {
        //Search for the item where the resource property is contains the current page.
        var selected = _navItems.Find(e=>resource.StartsWith(e.Resource));
        if (selected is not null) 
        {
            selected.IsSelected = true;
            //Disable items after selection
            for(int i = _navItems.IndexOf(selected) + 1; i < _navItems.Count; i++) _navItems[i].IsDisabled = true;
            //Enable navigation to items before selection
            for(int i =0 ; i <= _navItems.IndexOf(selected); i++) _navItems[i].IsDisabled = false;
        }

    }  
    public int GetSelectedIndex() 
    {
        var selected = _navItems.Find(e=>e.IsSelected);
        return selected is not null ? _navItems.IndexOf(selected) : -1;
    }

    public void UpdateResource(string newResource)
    {
        var selected = _navItems.Find(e=>e.IsSelected);
        if (selected is not null) selected.Resource = newResource;
    }

    public string? GetResourceAtIndex(int index)
    {
        return index >= 0 && index < _navItems.Count ? _navItems[index].Resource : null;
    }

}