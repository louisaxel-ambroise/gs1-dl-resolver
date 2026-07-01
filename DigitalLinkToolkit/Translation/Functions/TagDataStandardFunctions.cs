using DigitalLinkToolkit.Translation.Model.Tables;
using System.Text;

namespace DigitalLinkToolkit.Translation.Functions;

public class TagDataStandardFunctions
{
    public static string Parse(Row row, Bitstream binaryInput)
    {
        var result = new StringBuilder();
        var specSection = row.GetString("c");

        var firstComponent = specSection switch
        {
            "14.5.2" => ParseFixedBitLengthNumeric(row, binaryInput),
            "14.5.3" => ParsePrioritizedDate(binaryInput),
            "14.5.4" => ParseFixedLengthNumeric(row, binaryInput),
            "14.5.5" => ParseDelimitedOrTerminatedNumeric(row, binaryInput),
            "14.5.6" => ParseVariableLengthAlphanumeric(row, binaryInput),
            "14.5.7" => ParseSingleDataBit(binaryInput),
            "14.5.8" => Parse6DigitDateYYMMDD(binaryInput),
            "14.5.9" => Parse10DigitDateYYMMDDhhmm(binaryInput),
            "14.5.10" => ParseVariableFormatDateOrDateRange(binaryInput),
            "14.5.11" => ParseVariablePrecisionDateTime(binaryInput),
            "14.5.12" => ParseCountryCode(binaryInput),
            "14.5.13" => ParseVariableLengthNumericStringWithoutEncodingIndicator(row, binaryInput),
            "14.5.14" => ParseOptionalMinusSignIn1Bit(binaryInput),
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

    public static string Format(Row row, string value)
    {
        var result = new StringBuilder();
        var specSection = row.GetString("c");

        var firstComponent = specSection switch
        {
            "14.5.2" => FormatFixedBitLengthNumeric(row, value),
            "14.5.3" => FormatPrioritizedDate(value),
            "14.5.4" => FormatFixedLengthNumeric(row, value),
            "14.5.5" => FormatDelimitedOrTerminatedNumeric(row, value),
            "14.5.6" => FormatVariableLengthAlphanumeric(row, value),
            "14.5.7" => FormatSingleDataBit(value),
            "14.5.8" => Format6DigitDateYYMMDD(value),
            "14.5.9" => Format10DigitDateYYMMDDhhmm(value),
            "14.5.10" => FormatVariableFormatDateOrDateRange(value),
            "14.5.11" => FormatVariablePrecisionDateTime(value),
            "14.5.12" => FormatCountryCode(value),
            "14.5.13" => FormatVariableLengthNumericStringWithoutEncodingIndicator(row, value),
            "14.5.14" => FormatOptionalMinusSignIn1Bit(value),
            _ => throw new NotImplementedException()
        };
        result.Append(firstComponent);

        var secondComponentRow = row.ToSecondComponentRow();
        if (secondComponentRow is not null)
        {
            result.Append(Format(secondComponentRow, value));
        }

        return result.ToString();
    }

    private static string FormatOptionalMinusSignIn1Bit(string value)
    {
        return value switch
        {
            "" => "0",
            "-" => "1",
            _ => throw new InvalidOperationException("Invalid value for OptionalMinusSignIn1Bit")
        };
    }

    private static string FormatVariableLengthNumericStringWithoutEncodingIndicator(Row row, string value)
    {
        throw new NotImplementedException();
    }

    private static string FormatCountryCode(string value)
    {
        return string.Concat(value.Select(c => Convert.ToString(c - 'A', 2).PadLeft(6, '0')));
    }

    private static string FormatVariablePrecisionDateTime(string value)
    {
        var prefix = (value.Length / 2) % 4;

        return string.Concat(Convert.ToString(prefix, 2).PadLeft(2, '0'), FormatBinaryDate(value));
    }

    private static string FormatVariableFormatDateOrDateRange(string value)
    {
        return value.Length == 6
            ? string.Concat("0", FormatBinaryDate(value[..6]))
            : string.Concat("1", FormatBinaryDate(value[..6]), FormatBinaryDate(value[6..]));
    }

    private static string Format10DigitDateYYMMDDhhmm(string value)
    {
        return FormatBinaryDate(value);
    }

    private static string Format6DigitDateYYMMDD(string value)
    {
        return FormatBinaryDate(value);
    }

    private static string FormatBinaryDate(string value)
    {
        var bitLength = new[] { 7, 4, 5, 5, 6, 6 };
        var result = new StringBuilder();

        for(var i=0; 2*i<value.Length; i++)
        {
            result.Append(Convert.ToString(int.Parse(value.Substring(2*i, 2)), 2).PadLeft(bitLength[i], '0'));
        }

        return result.ToString();
    }

    private static string FormatSingleDataBit(string value)
    {
        return value;
    }

    private static string FormatVariableLengthAlphanumeric(Row row, string value)
    {
        var result = new StringBuilder();
        var length = Convert.ToString(value.Length, 2).PadLeft(row.GetNumber("g"), '0');
        
        result.Append("100").Append(length).Append(Alphabets.GetAsciiBinary(value));

        return result.ToString();
    }

    private static string FormatDelimitedOrTerminatedNumeric(Row row, string value)
    {
        throw new NotImplementedException();
    }

    private static string FormatFixedLengthNumeric(Row row, string value)
    {
        var result = new StringBuilder();

        for (var i = 0; i < value.Length; i++)
        {
            result.Append(Convert.ToString(int.Parse(value[i].ToString()), 2).PadLeft(4, '0'));
        }

        return result.ToString();
    }

    private static string FormatPrioritizedDate(string value)
    {
        // TODO: check values for table (11, 13, 15, 16, 17, 7006, 7007)
        return string.Concat("0000", FormatBinaryDate(value));
    }

    private static string FormatFixedBitLengthNumeric(Row row, string value)
    {
        throw new NotImplementedException();
    }

    public static string Parse(string encodingType, Bitstream binaryInput)
    {
        return encodingType switch
        {
            "dateYYMMDD" => Parse6DigitDateYYMMDD(binaryInput),
            _ => throw new NotImplementedException()
        };
    }

    public static string Format(string encodingType, string value)
    {
        return encodingType switch
        {
            "dateYYMMDD" => Format6DigitDateYYMMDD(value),
            _ => throw new NotImplementedException()
        };
    }

    private static string ParseVariablePrecisionDateTime(Bitstream binaryInput)
    {
        var prefix = Convert.ToInt32(binaryInput.Read(2), 2);

        return prefix switch
        {
            0 => ParseDateComponent([7, 4, 5, 5], binaryInput),
            1 => ParseDateComponent([7, 4, 5, 5, 6], binaryInput),
            2 => ParseDateComponent([7, 4, 5, 5, 6, 6], binaryInput),
            3 => ParseDateComponent([7, 4, 5], binaryInput),
            _ => throw new Exception("Invalid variable precision date time prefix."),
        };
    }

    private static string ParseDateComponent(int[] bits, Bitstream binaryInput)
    {
        return string.Concat(bits.Select(s => Convert.ToInt32(binaryInput.Read(s), 2).ToString("00")));
    }

    private static string ParseVariableLengthNumericStringWithoutEncodingIndicator(Row row, Bitstream binaryInput)
    {
        var length = Convert.ToInt32(binaryInput.Read(row.GetNumber("g")), 2);
        var bv = (int) Math.Ceiling(length * Math.Log(10) / Math.Log(2));

        var binary = binaryInput.Read(bv);

        return binary.ToBinaryValue().PadLeft(length, '0');
    }

    private static string ParseCountryCode(Bitstream binaryInput)
    {
        var first = 'A' + Convert.ToInt32(binaryInput.Read(6), 2);
        var second = 'A' + Convert.ToInt32(binaryInput.Read(6), 2);

        return $"{first}{second}";
    }

    private static string ParseOptionalMinusSignIn1Bit(Bitstream binaryInput)
    {
        return binaryInput.Read(1) == "1" ? "-" : "";
    }

    private static string ParseVariableFormatDateOrDateRange(Bitstream binaryInput)
    {
        var repetitions = Convert.ToInt32(binaryInput.Read(1), 2);
        var result = "";

        for (var i = 0; i <= repetitions; i++)
        {
            result += ParseDateComponent([7, 4, 5], binaryInput);
        }

        return result;
    }

    private static string Parse6DigitDateYYMMDD(Bitstream binaryInput)
    {
        return ParseDateComponent([7, 4, 5], binaryInput);
    }

    private static string Parse10DigitDateYYMMDDhhmm(Bitstream binaryInput)
    {
        return ParseDateComponent([7, 4, 5, 5, 6], binaryInput);
    }

    private static string ParseSingleDataBit(Bitstream binaryInput)
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

        return binaryInput.Read(bitLength).ToBinaryValue();
    }

    private static string ParsePrioritizedDate(Bitstream binaryInput)
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
                    var length = int.Parse(binaryInput.Read(row.GetNumber("g")).ToBinaryValue());
                    var bv = (int)Math.Ceiling(length * Math.Log(10) / Math.Log(2));
                    return binaryInput.Read(bv).ToBinaryValue();
                }
            case 1:
                {
                    var length = int.Parse(binaryInput.Read(row.GetNumber("g")).ToBinaryValue());
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
                    var length = int.Parse(binaryInput.Read(row.GetNumber("g")).ToBinaryValue());
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
                        var r = binaryInput.Read(16).ToBigInteger();
                        var i3 = (int) ((r - 1) % 40);
                        var i2 = (int) ((r - 1 - i3)/40 % 40);
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
