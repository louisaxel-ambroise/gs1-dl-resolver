using FasTnT.TagDataTranslation.Model.Tables;
using System.Text;
using System.Text.RegularExpressions;

namespace FasTnT.TagDataTranslation.Functions;

public class TagDataStandardFunctions
{
    public static string Parse(Row row, Bitstream binaryInput)
    {
        var result = new StringBuilder();
        var specSection = row.GetString("c");

        var firstComponent = specSection switch
        {
            "14.5.2" => ParseFixedBitLengthNumeric(row, binaryInput),
            "14.5.3" => ParsePrioritizedDate(row, binaryInput),
            "14.5.4" => ParseFixedLengthNumeric(row, binaryInput),
            "14.5.5" => ParseDelimitedOrTerminatedNumeric(row, binaryInput),
            "14.5.6" => ParseVariableLengthAlphanumeric(row, binaryInput),
            "14.5.7" => ParseSingleDataBit(row, binaryInput),
            "14.5.8" => Parse6DigitDateYYMMDD(row, binaryInput),
            "14.5.9" => Parse10DigitDateYYMMDDhhmm(row, binaryInput),
            "14.5.10" => ParseVariableFormatDateOrDateRange(row, binaryInput),
            "14.5.11" => ParseVariablePrecisionDateTime(row, binaryInput),
            "14.5.12" => ParseCountryCode(row, binaryInput),
            "14.5.13" => ParseVariableLengthNumericStringWithoutEncodingIndicator(row, binaryInput),
            "14.5.14" => ParseOptionalMinusSignIn1Bit(row, binaryInput),
            _ => throw new NotImplementedException()
        };
        result.Append(firstComponent);

        var secondComponentRow = row.ToSecondComponentRow();
        if(secondComponentRow is not null)
        {
            result.Append(Parse(secondComponentRow, binaryInput));
        }

        return result.ToString();
    }

    private static string ParseVariablePrecisionDateTime(Row row, Bitstream binaryInput)
    {
        var prefix = Convert.ToInt32(binaryInput.Read(2), 2);

        switch (prefix)
        {
            case 0:
                { 
                    var YY = Convert.ToInt32(binaryInput.Read(7), 2);
                    var MM = Convert.ToInt32(binaryInput.Read(4), 2);
                    var DD = Convert.ToInt32(binaryInput.Read(5), 2);
                    var hh = Convert.ToInt32(binaryInput.Read(5), 2);
                    return $"{YY}{MM}{DD}{hh}";
                }
            case 1:
                {
                    var YY = Convert.ToInt32(binaryInput.Read(7), 2);
                    var MM = Convert.ToInt32(binaryInput.Read(4), 2);
                    var DD = Convert.ToInt32(binaryInput.Read(5), 2);
                    var hh = Convert.ToInt32(binaryInput.Read(5), 2);
                    var mm = Convert.ToInt32(binaryInput.Read(6), 2);
                    return $"{YY}{MM}{DD}{hh}{mm}";
                }
            case 2:
                {
                    var YY = Convert.ToInt32(binaryInput.Read(7), 2);
                    var MM = Convert.ToInt32(binaryInput.Read(4), 2);
                    var DD = Convert.ToInt32(binaryInput.Read(5), 2);
                    var hh = Convert.ToInt32(binaryInput.Read(5), 2);
                    var mm = Convert.ToInt32(binaryInput.Read(6), 2);
                    var ss = Convert.ToInt32(binaryInput.Read(6), 2);
                    return $"{YY}{MM}{DD}{hh}{mm}{ss}";
                }
            case 3:
                {
                    var YY = Convert.ToInt32(binaryInput.Read(7), 2);
                    var MM = Convert.ToInt32(binaryInput.Read(4), 2);
                    var DD = Convert.ToInt32(binaryInput.Read(5), 2);
                    return $"{YY}{MM}{DD}";
                }
            default:
                throw new Exception("Invalid variable precision date time prefix.");
        }
    }

    private static string ParseVariableLengthNumericStringWithoutEncodingIndicator(Row row, Bitstream binaryInput)
    {
        var length = Convert.ToInt32(binaryInput.Read(row.GetNumber("g")), 2);
        var bv = (int) Math.Ceiling(length * Math.Log(10) / Math.Log(2));

        var binary = binaryInput.Read(bv);

        return Utils.ParseBinaryValue(binary).PadLeft(length, '0');
    }

    private static string ParseCountryCode(Row row, Bitstream binaryInput)
    {
        var first = 'A' + Convert.ToInt32(binaryInput.Read(6), 2);
        var second = 'A' + Convert.ToInt32(binaryInput.Read(6), 2);

        return $"{first}{second}";
    }

    private static string ParseOptionalMinusSignIn1Bit(Row row, Bitstream binaryInput)
    {
        return binaryInput.Read(1) == "1" ? "-" : "";
    }

    private static string ParseVariableFormatDateOrDateRange(Row row, Bitstream binaryInput)
    {
        var repetitions = Convert.ToInt32(binaryInput.Read(1), 2);
        var result = "";

        for(var i=0; i<= repetitions; i++)
        {
            var YY = Convert.ToInt32(binaryInput.Read(7), 2);
            var MM = Convert.ToInt32(binaryInput.Read(4), 2);
            var DD = Convert.ToInt32(binaryInput.Read(5), 2);

            result += $"{YY}{MM}{DD}";
        }

        return result;
    }

    private static string Parse6DigitDateYYMMDD(Row row, Bitstream binaryInput)
    {
        var YY = Convert.ToInt32(binaryInput.Read(7), 2);
        var MM = Convert.ToInt32(binaryInput.Read(4), 2);
        var DD = Convert.ToInt32(binaryInput.Read(5), 2);

        if (MM < 1 || MM > 12)
        {
            throw new Exception("Invalid month");
        }
        if (DD < 1 || DD > 31) // TODO: make a better validation of the month
        {
            throw new Exception("Invalid day");
        }

        return $"{YY}{MM}{DD}";
    }

    private static string Parse10DigitDateYYMMDDhhmm(Row row, Bitstream binaryInput)
    {
        var YY = Convert.ToInt32(binaryInput.Read(7), 2);
        var MM = Convert.ToInt32(binaryInput.Read(4), 2);
        var DD = Convert.ToInt32(binaryInput.Read(5), 2);
        var hh = Convert.ToInt32(binaryInput.Read(5), 2);
        var mm = Convert.ToInt32(binaryInput.Read(6), 2);

        if (MM < 1 || MM > 12)
        {
            throw new Exception("Invalid month");
        }
        if (DD < 1 || DD > 31) // TODO: make a better validation of the month
        {
            throw new Exception("Invalid day");
        }

        return $"{YY}{MM}{DD}{hh}{mm}";
    }

    private static string ParseSingleDataBit(Row row, Bitstream binaryInput)
    {
        return binaryInput.Read(1);
    }

    private static string ParseDelimitedOrTerminatedNumeric(Row row, Bitstream binaryInput)
    {
        var terminated = false;
        var result = "";

        while (!terminated)
        {
            var value = binaryInput.Read(4);

            switch (value)
            {
                case "1111":
                    terminated = true;
                    break;
                case "1110":
                    result += ParseVariableLengthAlphanumeric(row, binaryInput);
                    terminated = true;
                    break;
                default:
                    result += Convert.ToInt32(value, 10).ToString();
                    break;
            }
        }
     
        return result;
    }

    private static string ParseFixedBitLengthNumeric(Row row, Bitstream binaryInput)
    {
        var bitLength = row.GetNumber("e");

        return Utils.ParseBinaryValue(binaryInput.Read(bitLength));
    }

    private static string ParsePrioritizedDate(Row row, Bitstream binaryInput)
    {
        // TODO: validate table (11, 13, 15, 16, 17, 7006, 7007)
        binaryInput.Read(4);

        var year = Convert.ToInt32(binaryInput.Read(7), 2);
        var month = Convert.ToInt32(binaryInput.Read(4), 2);
        var day = Convert.ToInt32(binaryInput.Read(5), 2);

        if(month < 1 || month > 12)
        {
            throw new Exception("Invalid month");
        }
        if(day < 1 || day > 31) // TODO: make a better validation of the month
        {
            throw new Exception("Invalid day");
        }

        return $"{year}{month}{day}";
    }

    private static string ParseFixedLengthNumeric(Row row, Bitstream binaryInput)
    {
        var length = row.GetNumber("e");
        var value = new StringBuilder();

        for (var i = 0; i < length; i += 4)
        {
            value.Append(Convert.ToInt32(binaryInput.Read(4), 2));
        }

        return value.ToString();
    }

    private static string ParseVariableLengthAlphanumeric(Row row, Bitstream binaryInput)
    {
        var encoding = Convert.ToInt32(binaryInput.Read(row.GetNumber("f")), 2);

        switch (encoding)
        {
            case 0:
                {
                    var length = int.Parse(Utils.ParseBinaryValue(binaryInput.Read(row.GetNumber("g"))));
                    var bv = (int)Math.Ceiling(length * Math.Log(10) / Math.Log(2));
                    return Utils.ParseBinaryValue(binaryInput.Read(bv));
                }
            case 1:
                {
                    var length = int.Parse(Utils.ParseBinaryValue(binaryInput.Read(row.GetNumber("g"))));
                    var result = new StringBuilder(length);

                    for(var i=0; i<length; i++)
                    {
                        var value = Alphabets.GetAlpha(binaryInput.Read(4));
                        result.Append(value);
                    }

                    return result.ToString();
                }
            case 2:
                {
                    var length = int.Parse(Utils.ParseBinaryValue(binaryInput.Read(row.GetNumber("g"))));
                    var result = new StringBuilder(length);

                    for (var i = 0; i < length; i++)
                    {
                        var value = Alphabets.GetAlpha(binaryInput.Read(4));
                        result.Append(char.ToLower(value));
                    }

                    return result.ToString();
                }
            case 3:
                {
                    var length = Convert.ToInt32(binaryInput.Read(row.GetNumber("g")), 2);
                    var result = new StringBuilder(length);

                    for (var i = 0; i < length; i++)
                    {
                        var value = Alphabets.GetChar(binaryInput.Read(6));
                        result.Append(value);
                    }

                    return result.ToString();
                }
            case 4:
                {
                    var length = Convert.ToInt32(binaryInput.Read(row.GetNumber("g")), 2);
                    var result = new StringBuilder(length);

                    for (var i = 0; i < length; i++)
                    {
                        result.Append(Alphabets.GetAscii(binaryInput.Read(7)));
                    }

                    return result.ToString();
                }
            case 5:
                {
                    var length = Convert.ToInt32(binaryInput.Read(row.GetNumber("g")), 2);
                    var result = new StringBuilder(length);
                    var n = length % 3 == 0 ? length / 3 : (int)Math.Ceiling(length / 3m);

                    for (var i = 0; i < n; i++)
                    {
                        var r = Utils.ParseBigInteger(binaryInput.Read(16));
                        var i3 = (int) ((r - 1) % 40);
                        var i2 = (int) (((r - 1 - i3)/40) % 40);
                        var i1 = (int)((r-1 - i3 - 40*i2)/1600);

                        result.Append(Alphabets.GetCode40(i1));

                        if (i2 > 0) result.Append(Alphabets.GetCode40(i2));
                        if (i3 > 0) result.Append(Alphabets.GetCode40(i3));
                    }

                    return result.ToString();
                }
            default:
                throw new Exception("Unsupported encoding.");
        }
    }
}

public partial class FunctionUtils
{
    private static readonly Regex _regex = FunctionRegex();
    
    public static Func<Dictionary<string,string>, string> ParseFunction(string functionDefinition)
    {
        var match = _regex.Match(functionDefinition);
        var function = match.Groups["function"].Value;
        var parameters = match.Groups["params"].Value.Split(',');

        return function switch
        {
            "SUBSTR" => fields => Substring(fields, parameters),
            "CONCAT" => fields => Concat(fields, parameters),
            "LENGTH" => fields => Length(fields, parameters),
            // split URN and URL encoding if needed
            "URNENCODE" or "URLENCODE" => fields => UrlEncode(fields, parameters),
            "URNDECODE" or "URLDECODE" => fields => UrlDecode(fields, parameters),
            "GS1CHECKSUM" => fields => Checksum(fields, parameters),
            _ => throw new ArgumentException("Invalid function definition.")
        };
    }

    private static string Substring(Dictionary<string, string> fields, string[] parameters)
    {
        if (!fields.TryGetValue(parameters[0], out var field))
        {
            throw new Exception($"Field {parameters[0]} not found.");
        }

        if (parameters.Length == 2)
        {
            var startIndex = ParseParameter(parameters[1], fields);

            return field.Substring(startIndex);
        }
        if (parameters.Length == 3)
        {
            var startIndex = ParseParameter(parameters[1], fields);
            var endIndex = ParseParameter(parameters[2], fields);

            return field.Substring(startIndex, endIndex);
        }

        throw new Exception("Invalid number of parameters for SUBSTRING function.");
    }

    private static string Concat(Dictionary<string, string> fields, string[] parameters)
    {
        return string.Concat(parameters.Select(p => fields[p]));
    }

    private static string Length(Dictionary<string, string> fields, string[] parameters)
    {
        return fields[parameters[0]].Length.ToString();
    }

    public static string Checksum(Dictionary<string, string> fields, string[] parameters)
    {
        var value = fields[parameters[0]];
        var weightedSum = 0;

        for (var i = 0; i < value.Length; i++)
        {
            var weight = i % 2 == 0 ? 3 : 1;
            weightedSum += (value[i] - '0') * weight;
        }

        var checkDigit = 10 - weightedSum % 10;

        return $"{checkDigit % 10}";
    }

    public static string UrlEncode(Dictionary<string, string> fields, string[] parameters)
    {
        return Uri.EscapeDataString(fields[parameters[0]]);
    }

    public static string UrlDecode(Dictionary<string, string> fields, string[] parameters)
    {
        return Uri.UnescapeDataString(fields[parameters[0]]);
    }

    private static int ParseParameter(string value, Dictionary<string, string> fields)
    {
        if (int.TryParse(value, out var intValue))
        {
            return intValue;
        }
        else
        {
            return int.Parse(fields[value]);
        }
    }

    [GeneratedRegex("^(?<function>[a-zA-Z0-9]+)\\((?<params>[a-zA-Z0-9,]+)\\)$")]
    private static partial Regex FunctionRegex();
}
