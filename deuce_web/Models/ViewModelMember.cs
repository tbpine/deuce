using deuce;



public class ViewModelMember
{
    private Account _account = new();

    private ISideMenuHandler _sideMenuHandler;

    public Account Account { get => _account; set => _account = value; }

    public IEnumerable<NavItem> NavItems { get => _sideMenuHandler.NavItems; }

    public ViewModelMember(ISideMenuHandler sidemenu)
    {
        _sideMenuHandler = sidemenu;
    }   
}