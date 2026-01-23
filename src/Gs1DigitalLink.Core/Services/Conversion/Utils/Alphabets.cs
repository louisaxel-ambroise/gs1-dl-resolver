using System.Text;

namespace Gs1DigitalLink.Core.Services.Conversion.Utils;

internal static class Alphabets
{
    public static char GetChar(ReadOnlySpan<char> input) => Base64UrlSafe.ElementAt(Convert.ToInt32(input.ToString(), 2));
    public static char GetAlpha(ReadOnlySpan<char> input) => Alpha.ElementAt(Convert.ToInt32(input.ToString(), 2));
    public static string GetAlphaBinary(string input) => string.Concat(input.Select(GetAlphaBinary));
    public static string GetBase64Binary(string input) => string.Concat(input.Select(GetBinary));
    public static string GetBinary(char input) => Convert.ToString(Base64UrlSafe.IndexOf(input), 2).PadLeft(6, '0');
    public static string GetAlphaBinary(char input) => Convert.ToString(Alpha.IndexOf(input, StringComparison.OrdinalIgnoreCase), 2).PadLeft(4, '0');

    private static readonly string Alpha = "0123456789ABCDEF";
    private static readonly string Base64UrlSafe = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

    public static StringBuilder GetChars(this StringBuilder builder)
    {
        var parsed = new StringBuilder(builder.Length / 6);
        var buffer = new char[6];
        var length = 0;

        foreach (var chunk in builder.GetChunks())
        {
            foreach (var c in chunk.Span)
            {
                buffer[length++] = c;

                if (length == 6)
                {
                    parsed.Append(GetChar(buffer));
                    length = 0;
                }
            }
        }

        return parsed;
    }
}