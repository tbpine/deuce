namespace deuce.ext;

public static class MemberExt
{
    /// <summary>
    /// Populates the First, Middle, and Last names of a Player object from an array of strings.
    /// </summary>
    /// <param name="member">The Player object to populate.</param>
    /// <param name="nameParts">An array of strings representing the player's name.</param>
    public static void PopulateNames(this Member member, string[] nameParts)
    {
        if (nameParts == null || nameParts.Length == 0)
        {
            member.First = null;
            member.Middle = null;
            member.Last = null;
            return;
        }

        member.First = nameParts.Length > 0 ? nameParts[0] : "";
        member.Middle = nameParts.Length > 2 ? string.Join(" ", nameParts[1..^1]) : "";
        member.Last = nameParts.Length > 1 ? nameParts[^1] : "";
    }

}