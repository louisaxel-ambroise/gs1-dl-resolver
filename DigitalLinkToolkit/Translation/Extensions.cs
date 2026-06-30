using System.Numerics;

namespace DigitalLinkToolkit.Translation;

internal static class Extensions
{
    public static string ToBinaryValue(this string value)
    {
        return value.ToBigInteger().ToString();
    }

    public static BigInteger ToBigInteger(this string value)
    {
        BigInteger res = 0;

        foreach (char c in value)
        {
            res <<= 1;
            res += c == '1' ? 1 : 0;
        }

        return res;
    }
}