/// <summary>
/// Get / Set values from a browser's session
/// </summary>
public class SessionProxy : ISessionProxy
{
    //Define session keys in one place.
    private const string Key_Current_Session = "CurrentTournament";
    private const string Key_Current_OrganizationId = "OrganizationId";
    private const string Key_Current_EntryType = "EntryType";
    //Keep reference to the browser session
    private  ISession? _session;

    /// <summary>
    /// Default constructor
    /// </summary>
    public SessionProxy()
    {

    }
    
    public SessionProxy(ISession session) => _session = session;
    public ISession? Session {  set=>_session = value; }
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

    /// <summary>
    /// Clear  the current browser session
    /// </summary>
    public void Clear()
    {
        _session?.Clear();
    }

}