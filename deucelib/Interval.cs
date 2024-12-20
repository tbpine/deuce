namespace deuce.lib;

/// <summary>
/// Minutes, Hours, Day, Week, Month etc.
/// </summary>
/// <param name="Id">DB primary key</param>
/// <param name="Label">A string category</param>
public record Interval (int Id, string Label);