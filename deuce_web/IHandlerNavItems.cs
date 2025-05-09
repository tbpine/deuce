public interface IHandlerNavItems
{
    void Set(string resource);
    void SetControllerAction(string controller, string action);
    int GetSelectedIndex();
    void UpdateResource(string newResource);
    string? GetResourceAtIndex(int index);

    IEnumerable<NavItem> NavItems { get; }

}