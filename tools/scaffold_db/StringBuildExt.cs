namespace deuce.extensions;
using System.Text;

static class StringBuilderExt
{
    public static void  TrimRight(this StringBuilder sb, int length)
    {
        sb.Remove(sb.Length - length, length);
    }

    public static void  TrimLeft(this StringBuilder sb, int length)
    {
        sb.Remove(0, length);
    }

}