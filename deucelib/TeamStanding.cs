namespace deuce;

/// <summary>
/// Represents a team's current standing in the tournament.
/// </summary>
public class TeamStanding
{
    /// <summary>
    /// Gets or sets the unique identifier for the team standing.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the team associated with this standing.
    /// </summary>
    public int TeamId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the tournament associated with this standing.
    /// </summary>
    public int Tournament { get; set; }

    /// <summary>
    /// Gets or sets the team object associated with this standing.
    /// </summary>
    public Team Team { get; set; } = new Team();

    /// <summary>
    /// Gets or sets the number of wins.
    /// </summary>
    public int Wins { get; set; }

    /// <summary>
    /// Gets or sets the number of losses.
    /// </summary>
    public int Losses { get; set; }

    /// <summary>
    /// Gets or sets the number of draws.
    /// </summary>
    public int Draws { get; set; }

    /// <summary>
    /// Gets or sets the points earned by the team.
    /// </summary>
    public double Points { get; set; }

    /// <summary>
    /// Gets or sets the position of the team in the standings.
    /// </summary>
    public int Position { get; set; }
}