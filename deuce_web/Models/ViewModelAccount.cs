using deuce;

/// <summary>
/// Data for  account controller views
/// </summary>
public class ViewModelAccount
{
    private Account _account = new();
    private Organization _organization = new();
    private Member _member = new();

    public Account Account { get => _account; }
    public Organization Organization { get => _organization; }
    public Member Member { get => _member; }

    //Name entry
    public string? Name { get; set; }

    /// <summary>
    /// Construct with references
    /// </summary>
    public ViewModelAccount()
    {
    }

}