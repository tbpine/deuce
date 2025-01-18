namespace deuce.ext;
using System.Data.Common;

/// <summary>
/// Match class extensions.
/// </summary>
public static class PlayerExt
{

  /// <summary>
  /// True is a player object has enough data for storage
  /// </summary>
  /// <param name="player">Player object</param>
  /// <returns>T value</returns>
  public static bool IsStorable(this Player player)
  {
        //Has first and last name
        bool hasDetails  = !String.IsNullOrEmpty(player.First) && !String.IsNullOrEmpty(player.Last);
        bool isRegistered = player.Id > 1;
        
        return hasDetails || isRegistered;
  }

  


}