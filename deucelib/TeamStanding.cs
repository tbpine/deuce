namespace deuce;

/// <summary>
/// Represents a team's current standing in the tournament.
/// </summary>
public class TeamStanding
{
    public Team Team { get; set; } = new Team();
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public double Points { get; set; }
    public int Position { get; set; }
}