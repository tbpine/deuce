/// <summary>
/// Represents a score entity with details about a match, players, and related metadata.
/// </summary>
public class Score
{
    /// <summary>
    /// Gets or sets the unique identifier for the score.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the tournament associated with the score.
    /// </summary>
    public int Tournament { get; set; }

    /// <summary>
    /// Gets or sets the permutation index, which may represent a specific configuration or scenario.
    /// </summary>
    public int Permutation { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the match associated with the score.
    /// </summary>
    public int Match { get; set; }

    /// <summary>
    /// Gets or sets the score of the home team  in the match.
    /// </summary>
    public int Home { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the away team in the match.
    /// </summary>
    public int Away { get; set; }

    /// <summary>
    /// Gets or sets additional notes or comments about the score.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Score"/> class.
    /// </summary>
    public Score()
    {
    }
}
