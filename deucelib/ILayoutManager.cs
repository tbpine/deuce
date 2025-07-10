namespace deuce;


/// <summary>
/// Interface for layout managers.
/// </summary>
public interface ILayoutManager
{
    // Define methods and properties that all LayoutManagers should implement
    void Initialize();
    object ArrangeLayout(Tournament tournament);
    // Add more methods/properties as needed
}
