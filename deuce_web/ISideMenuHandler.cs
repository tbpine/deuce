/// <summary>
/// Defines methods/properties to manage
/// side menu navigation in the account 
/// section.
/// </summary>
public interface ISideMenuHandler
{
    
    void Set(string resource);
    int GetSelectedIndex();
    void UpdateResource(string newResource);
    string? GetResourceAtIndex(int index);

    IEnumerable<NavItem> NavItems { get; }
}