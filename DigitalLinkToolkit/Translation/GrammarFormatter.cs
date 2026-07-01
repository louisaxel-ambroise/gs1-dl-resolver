using DigitalLinkToolkit.Translation.Functions;
using DigitalLinkToolkit.Translation.Model.EPCs;
using DigitalLinkToolkit.Translation.Model.Tables;
using System.Numerics;
using System.Text;

namespace DigitalLinkToolkit.Translation;

public sealed class GrammarFormatter(List<Table> tables)
{
    internal string Format(Level level, Option option, Dictionary<string, string> parameters)
    {
        if (string.IsNullOrEmpty(option.Grammar))
            return string.Empty;

        var builder = new StringBuilder(option.Grammar.Length);
        var length = option.Grammar.Length;
        var index = 0;

        while (index < length)
        {
            var c = option.Grammar[index];

            if (c == '\'')
            {
                var close = option.Grammar.IndexOf('\'', index + 1);
                if (close < 0)
                    throw new FormatException("Unterminated quoted literal in grammar.");

                var literalLength = close - (index + 1);
                if (literalLength > 0)
                    builder.Append(option.Grammar, index + 1, literalLength);

                index = close + 1;
            }
            else if (char.IsWhiteSpace(c))
            {
                index++;
            }
            else
            {
                var nextSpace = option.Grammar.IndexOf(' ', index);
                if (nextSpace == -1)
                {
                    nextSpace = length;
                }

                var keyLength = nextSpace - index;
                var key = option.Grammar.Substring(index, keyLength);

                FormatValue(builder, option, parameters, key);
                index = nextSpace;
            }
        }

        return builder.ToString();
    }

    private void FormatValue(StringBuilder builder, Option option, Dictionary<string, string> parameters, string key)
    {
        var field = option.Fields.SingleOrDefault(f => f.Name == key);

        if (key == "encodedAI" && option.EncodedAIs.Any())
        {
            foreach (var encodedAI in option.EncodedAIs)
            {
                builder.Append(Format(encodedAI, parameters));
            }
        }
        else if (!parameters.TryGetValue(key, out var value))
        {
            throw new Exception($"No parameter with name {key} was found");
        }
        else
        {
            var formattedValue = field is null
                ? value
                : Format(field, value);
            builder.Append(formattedValue);
        }
    }

    private string Format(EncodedAI encodedAI, Dictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue(encodedAI.Name, out var value))
        {
            throw new Exception($"No parameter with name {encodedAI.Name} was found");
        }

        var row = tables.Single(t => t.TableId == "F").Rows.Single(r => r.GetString("a") == encodedAI.AI);

        return TagDataStandardFunctions.Format(row, value);
    }

    private static string? Format(Field field, string value)
    {
        if(field.Encoding is not null)
        {
            return TagDataStandardFunctions.Format(field.Encoding, value);
        }
        if (field.BitLength is not null)
        {
            if(BigInteger.TryParse(value, out var parsedValue))
            {
                var formatted = BigIntegerToBinaryString(parsedValue);

                if(formatted.Length < field.BitLength)
                {
                    formatted = field.BitPadDir == Direction.Left
                        ? formatted.PadLeft(field.BitLength.Value, (field.PadChar ?? "0").ElementAt(0))
                        : formatted.PadRight(field.BitLength.Value, (field.PadChar ?? "0").ElementAt(0));
                }

                return formatted;
            }
        }

        return value;
    }

    static string BigIntegerToBinaryString(BigInteger x)
    {
        if (x.IsZero)
            return "0";

        var srcBytes = x.ToByteArray();
        Span<char> dstBytes = stackalloc char[(int)x.GetBitLength()];

        int srcLoc = srcBytes.Length - 1;
        int dstLoc = 0;

        if (srcBytes[srcLoc] == 0) srcLoc--;
        int msb = BitOperations.Log2(srcBytes[srcLoc]);
        byte b = srcBytes[srcLoc--];
        for (int j = msb; j >= 0; j--)
        {
            dstBytes[dstLoc++] = (char)('0' + ((b >> j) & 1));
        }

        for (; srcLoc >= 0; srcLoc--)
        {
            byte b2 = srcBytes[srcLoc];
            for (int j = 7; j >= 0; j--)
            {
                dstBytes[dstLoc++] = (char)('0' + ((b2 >> j) & 1));
            }
        }

        return dstBytes.ToString();
    }
}