using System.Text;

namespace DigitalLinkToolkit.Translation.Functions;

internal static class Alphabets
{
    public static char GetChar(ReadOnlySpan<char> input) => Base64UrlSafe.ElementAt(Convert.ToInt32(input.ToString(), 2));
    public static char GetAlpha(ReadOnlySpan<char> input) => Alpha.ElementAt(Convert.ToInt32(input.ToString(), 2));
    public static char GetAscii(ReadOnlySpan<char> input) => char.ConvertFromUtf32(Convert.ToInt32(input.ToString(), 2))[0];
    public static string GetAlphaBinary(string input) => string.Concat(input.Select(GetAlphaBinary));
    public static string GetAsciiBinary(string input) => string.Concat(input.Select(GetAsciiBinary));
    public static string GetBase64Binary(string input) => string.Concat(input.Select(GetBinary));
    public static string GetBinary(char input) => Convert.ToString(Base64UrlSafe.IndexOf(input), 2).PadLeft(6, '0');
    public static string GetAlphaBinary(char input) => Convert.ToString(Alpha.IndexOf(input, StringComparison.OrdinalIgnoreCase), 2).PadLeft(4, '0');
    public static string GetAsciiBinary(char input) => Convert.ToString(char.ConvertToUtf32(input.ToString(), 0), 2).PadLeft(7, '0');
    public static char GetCode40(int index) => Code40.ElementAt(index);
    
    private static readonly string Alpha = "0123456789ABCDEF";
    private static readonly string Base64UrlSafe = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
    private static readonly string Code40 = "_ABCDEFGHIJKLMNOPQRSTUVWXYZ-.:0123456789";

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