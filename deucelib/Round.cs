namespace deuce;

/// <summary>
/// Represents a round in a tournament, containing permutations (matches/games) and associated metadata.
/// A round is a collection of matches that occur at the same stage of a tournament.
/// </summary>
public class Round
{
    private List<Permutation> _perms = new();
    private Tournament? _tournament;
    private Round? _playoff;

    private int _idx;
    string _label = string.Empty;

    /// <summary>
    /// Gets a read-only collection of permutations (matches/games) in this round.
    /// </summary>
    /// <value>A read-only list of <see cref="Permutation"/> objects representing the matches in this round.</value>
    public IReadOnlyList<Permutation> Permutations { get => _perms; }
    
    /// <summary>
    /// Gets or sets the index/position of this round within the tournament structure.
    /// </summary>
    /// <value>An integer representing the sequential position of this round.</value>
    public int Index { get=>_idx; set=>_idx = value; }
    
    /// <summary>
    /// Gets or sets the display label for this round. If no custom label is set, returns the index as a string.
    /// </summary>
    /// <value>A string representing the display name of the round (e.g., "Quarter Finals", "Round 1").</value>
    public string Label { get => string.IsNullOrEmpty(_label) ? $"{_idx}" : _label; set=> _label = value; }
    
    /// <summary>
    /// Gets or sets the tournament that this round belongs to.
    /// </summary>
    /// <value>The <see cref="Tournament"/> object this round is associated with, or null if not assigned.</value>
    public Tournament? Tournament  {get { return _tournament; } set { _tournament = value; }}
    
    /// <summary>
    /// Gets or sets the playoff round associated with this round, if applicable.
    /// </summary>
    /// <value>A <see cref="Round"/> object representing a playoff or tiebreaker round, or null if none exists.</value>
    /// <summary>
    /// Gets or sets the playoff round associated with this round, if applicable.
    /// </summary>
    /// <value>A <see cref="Round"/> object representing a playoff or tiebreaker round, or null if none exists.</value>
    public Round? Playoff { get => _playoff; set => _playoff = value; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Round"/> class with the specified index.
    /// </summary>
    /// <param name="idx">The index/position of this round within the tournament.</param>
    public Round(int idx)
    {
        _idx = idx;
    }

    /// <summary>
    /// Adds a permutation (match/game) to this round if it doesn't already exist.
    /// </summary>
    /// <param name="obj">The <see cref="Permutation"/> to add to this round.</param>
    /// <remarks>Duplicate permutations will not be added.</remarks>
    public void AddPerm(Permutation obj)
    {
        if (!_perms.Contains(obj)) _perms.Add(obj);
    } 

    /// <summary>
    /// Retrieves a permutation at the specified index within this round.
    /// </summary>
    /// <param name="index">The zero-based index of the permutation to retrieve.</param>
    /// <returns>The <see cref="Permutation"/> at the specified index, or null if the index is out of range.</returns>
    public Permutation? GetAtIndex(int index)=>_perms.ElementAtOrDefault(index);

    /// <summary>
    /// Adds multiple permutations to this round in a single operation.
    /// </summary>
    /// <param name="range">A collection of <see cref="Permutation"/> objects to add to this round.</param>
    /// <remarks>This method does not check for duplicates when adding the range.</remarks>
    public void AddRange(IEnumerable<Permutation> range) =>_perms.AddRange(range);


}