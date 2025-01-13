/// <summary>
/// Get / Set values from a browser's session
/// </summary>
public class SessionProxy : ISessionProxy
{
    //Define session keys in one place.
    private const string Key_Current_Session = "CurrentTournament";
    //Keep reference to the browser session
    private  readonly ISession? _session;

    public SessionProxy(ISession session)=>_session = session;
    public int TournamentId
    {
        get=>_session?.GetInt32(Key_Current_Session)??0;
        set=>_session?.SetInt32(Key_Current_Session, value);
    }
}