using System.Numerics;
using System.Text;

namespace Gs1DigitalLink.Core.Services.Conversion.Utils;

internal static class Extensions
{
    internal static bool IsNumeric(this byte value)
    {
        return value >> 4 <= 9 && (value & 0x0F) <= 9;
    }

    internal static bool IsLowerCaseHex(this string value)
        => value.All(x => (x >= '0' && x <= '9') || (x >= 'a' && x <= 'f'));

    internal static bool IsUpperCaseHex(this string value)
        => value.All(x => (x >= '0' && x <= '9') || (x >= 'A' && x <= 'F'));

    internal static bool IsUriSafeBase64(this string value)
        => value.All(x => (x >= '0' && x <= '9') || (x >= 'A' && x <= 'z') || x == '-' || x == '_');

    internal static bool IsNumeric(this string value)
        => value.All(x => x >= '0' && x <= '9');

    internal static string ToNumericString(this byte value)
    {
        if (!value.IsNumeric())
        {
            throw new ArgumentOutOfRangeException($"value is expected to be numeric: {value:X1}");
        }

        return value.ToString("X1");
    }

    internal static string ToBinaryString(this BigInteger bigint)
    {
        var bytes = bigint.ToByteArray();
        var idx = bytes.Length - 1;
        var base2 = new StringBuilder(bytes.Length * 8);
        var binary = Convert.ToString(bytes[idx], 2);

        if (binary[0] != '0' && bigint.Sign == 1)
        {
            base2.Append('0');
        }

        base2.Append(binary);

        for (idx--; idx >= 0; idx--)
        {
            base2.Append(Convert.ToString(bytes[idx], 2).PadLeft(8, '0'));
        }

        return base2.ToString();
    }
}