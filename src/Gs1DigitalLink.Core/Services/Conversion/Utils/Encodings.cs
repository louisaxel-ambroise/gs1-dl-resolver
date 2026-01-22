using System.Globalization;
using System.Numerics;
using System.Text;

namespace Gs1DigitalLink.Core.Services.Conversion.Utils;

internal record Encodings(Func<int, BitStream, string> Read)
{
    public static Encodings[] Values => [Numeric, LowerAlpha, UpperAlpha, Chars, ASCII];

    public static readonly Encodings Numeric = new((length, stream) =>
    {
        var bitsToRead = (int)Math.Ceiling(length * Math.Log(10) / Math.Log(2));
        stream.Buffer(bitsToRead);

        return BigInteger.Parse(stream.Current.ToString(), NumberStyles.BinaryNumber).ToString().PadLeft(length, '0');
    });

    public static readonly Encodings LowerAlpha = new((length, stream) =>
    {
        var chars = Enumerable.Range(1, length).Select(_ =>
        {
            stream.Buffer(4);
            return Alphabets.GetAlpha(stream.Current);
        });

        return new string([.. chars.Select(char.ToLower)]);
    });

    public static readonly Encodings UpperAlpha = new((length, stream) =>
    {
        var chars = Enumerable.Range(1, length).Select(_ =>
        {
            stream.Buffer(4);
            return Alphabets.GetAlpha(stream.Current);
        });

        return new string([.. chars.Select(char.ToUpper)]);
    });

    public static readonly Encodings Chars = new((length, stream) =>
    {
        var chars = Enumerable.Range(1, length).Select(_ =>
        {
            stream.Buffer(6);
            return Alphabets.GetChar(stream.Current);
        });

        return new string([.. chars]);
    });

    public static readonly Encodings ASCII = new((length, stream) =>
    {
        var bytes = Enumerable.Range(1, length).Select(_ =>
        {
            stream.Buffer(7);
            return Convert.ToByte(stream.Current.ToString(), 2);
        });

        return Encoding.ASCII.GetString([.. bytes]);
    });
}