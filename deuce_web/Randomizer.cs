using System.Text;

public class Randomizer
{

    /// <summary>
    /// Get a random string of a specified length
    /// </summary>
    /// <param name="length">Length of string</param>
    /// <returns></returns>
    public static string GetRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        StringBuilder result = new StringBuilder(length);
        Random random = new Random((int)DateTime.Now.Ticks);

        for (int i = 0; i < length; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }
        return result.ToString();
    }
}
