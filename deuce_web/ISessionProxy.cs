/// <summary>
/// Defines a session proxy class
/// </summary>
public interface ISessionProxy
{
    public int TournamentId { get; set; }
    public int OrganizationId { get; set; }
    public int EntryType { get; set; }
    public int CurrentAccount  { get; set; }
    
    //Clear the proxy
    public void Clear();

}
