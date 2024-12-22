namespace deuce.ext;
using System.Data.Common;

/// <summary>
/// Match class extensions.
/// </summary>
public static class DbDataReaderExt
{

  /// <summary>
  /// Get any type "T" from a column
  /// </summary>
  /// <param name="reader">Date reader</param>
  /// <param name="col">Column name</param>
  /// <returns>T value</returns>
  public static T Parse<T>(this DbDataReader reader, string col)
  {
    int ordinal = reader.GetOrdinal(col);

    if (reader.IsDBNull(ordinal)) return default(T)!;

    return reader.GetFieldValue<T>(ordinal);
    
  }


}