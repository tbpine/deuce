namespace deuce;

/// <summary>
/// Where cause of a select statement
/// </summary>
public class Filter
{

    public int ClubId { get; set; }
    public int TournamentId { get; set; }
    public string? TournamentLabel { get; set; }
    public int CountryCode { get; set; }
    public int Round { get; set; }
    
    public Filter()
    {

    }

}