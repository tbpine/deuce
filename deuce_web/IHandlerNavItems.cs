public interface IHandlerNavItems
{
    void Set(string resource);
    void SetControllerAction(string controller);
    int GetSelectedIndex();
    void UpdateResource(string newResource);
    string? GetResourceAtIndex(int index);

    IEnumerable<NavItem> NavItems { get; }

}