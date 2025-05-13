using deuce;
/// <summary>
/// Get / Set values from a browser's session
/// </summary>
public class SessionProxy : ISessionProxy
{
    //Define session keys in one place.
    private const string Key_Current_Session = "CurrentTournament";
    private const string Key_Current_OrganizationId = "OrganizationId";
    private const string Key_Current_EntryType = "EntryType";
    private const string Key_Current_Current_Round = "CurrentRound";
    private const string Key_Current_Account = "Account";
    private const string Key_Current_TeamSize = "TeamSize";

    //Keep reference to the browser session
    private ISession? _session;

    public SessionProxy()
    {

    }
    /// <summary>
    /// This constructor is for the class "BasePageModelWizard".
    /// It doesn't dependency inject the "SessionProxy"class.
    /// </summary>
    /// <param name="session">Web browser Session </param>
    public SessionProxy(ISession session) => _session = session;

    public ISession? Session { set => _session = value; }
    public int TournamentId
    {
        get => _session?.GetInt32(Key_Current_Session) ?? 0;
        set => _session?.SetInt32(Key_Current_Session, value);
    }

    public int OrganizationId
    {
        get => _session?.GetInt32(Key_Current_OrganizationId) ?? 1;
        set => _session?.SetInt32(Key_Current_OrganizationId, value);
    }

    public int EntryType
    {
        get => _session?.GetInt32(Key_Current_EntryType) ?? 1;
        set => _session?.SetInt32(Key_Current_EntryType, value);
    }

    public int CurrentRound
    {
        get => _session?.GetInt32(Key_Current_Current_Round) ?? 1;
        set => _session?.SetInt32(Key_Current_Current_Round, value);
    }

     public int CurrentAccount
    {
        get => _session?.GetInt32(Key_Current_Account)?? 0;
        set => _session?.SetInt32(Key_Current_Account, value);
    }

     public int TeamSize
    {
        get => _session?.GetInt32(Key_Current_TeamSize)?? 0;
        set => _session?.SetInt32(Key_Current_TeamSize, value);
    }


    /// <summary>
    /// Clear  the current browser session
    /// </summary>
    public void Clear()
    {
        _session?.Clear();
    }

}