/// <summary>
/// Define left side nav menu
/// </summary>
public class TourWizardNav : IHandlerNavItems
{
    private List<NavItem> _navItems = new();

    public IEnumerable<NavItem> NavItems { get => _navItems; }

    /// <summary>
    /// Make left side navigations
    /// </summary>
    public TourWizardNav()
    {
        _navItems.Add(new NavItem("Tournament Details", "/TournamentDetail", false, true,"",  "TDetail", "Index"));
        _navItems.Add(new NavItem("Venue", "/TournamentVenue", false, true,"", "TVenue", "Index"));
        //Both team and player formats
        _navItems.Add(new NavItem("Format", "/TournamentFormat", false, true, "", "TFormatTeam", "Index"));
        _navItems.Add(new NavItem("Players", "/TournamentPlayers", false, true, "", "TPlayers", "Index"));
        _navItems.Add(new NavItem("Schedule", "/TournamentSchedule", false, true, "", "TSchedule", "Index"));
        _navItems.Add(new NavItem("Fee", "/TournamentFee", false, true, "", "TFee", "Index"));
    }

    /// <summary>
    /// Set the active menu selection
    /// </summary>
    /// <param name="resource">Resource path</param>
    public void Set(string resource)
    {
        //Search for the item where the resource property is contains the current page.
        var selected = _navItems.Find(e => resource.StartsWith(e.Resource));
        if (selected is not null)
        {
            selected.IsSelected = true;
            //Disable items after selection
            for (int i = _navItems.IndexOf(selected) + 1; i < _navItems.Count; i++) _navItems[i].IsDisabled = true;
            //Enable navigation to items before selection
            for (int i = 0; i <= _navItems.IndexOf(selected); i++) _navItems[i].IsDisabled = false;
        }

    }
    /// <summary>
    /// Set the active menu selection by controller and action
    /// </summary>
    /// <param name="controller">Controller name</param>
    /// <param name="action">Action name</param>
    public void SetControllerAction(string controller)
    {
        //Search for the item where the resource property is contains the current page.
        var selected = _navItems.Find(e =>  e.Controller.StartsWith(controller));
        if (selected is not null)
        {
            selected.IsSelected = true;
            //Disable items after selection
            for (int i = _navItems.IndexOf(selected) + 1; i < _navItems.Count; i++) _navItems[i].IsDisabled = true;
            //Enable navigation to items before selection
            for (int i = 0; i <= _navItems.IndexOf(selected); i++) _navItems[i].IsDisabled = false;
        }


    }
    public int GetSelectedIndex()
    {
        var selected = _navItems.Find(e => e.IsSelected);
        return selected is not null ? _navItems.IndexOf(selected) : -1;
    }

    public void UpdateResource(string newResource)
    {
        var selected = _navItems.Find(e => e.IsSelected);
        if (selected is not null) selected.Resource = newResource;
    }

    public string? GetResourceAtIndex(int index)
    {
        return index >= 0 && index < _navItems.Count ? _navItems[index].Resource : null;
    }
}