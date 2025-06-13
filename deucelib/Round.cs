namespace deuce;

/// <summary>
/// Rounds
/// </summary>
public class Round
{
    private List<Permutation> _perms = new();
    private Tournament? _tournament;

    private int _idx;
    string _label = string.Empty;

    public IEnumerable<Permutation> Permutations { get => _perms; }
    public int Index { get=>_idx; set=>_idx = value; }
    public string Label { get => string.IsNullOrEmpty(_label) ? $"{_idx + 1}" : _label; set=> _label = value; }
    public Tournament? Tournament  {get { return _tournament; } set { _tournament = value; }}
    

    public Round(int idx)
    {
        _idx = idx;
    }

    public void AddPerm(Permutation obj)
    {
        if (!_perms.Contains(obj)) _perms.Add(obj);
    } 

    public Permutation GetAtIndex(int index)=>_perms[index];

    public void AddRange(IEnumerable<Permutation> range) =>_perms.AddRange(range);

}