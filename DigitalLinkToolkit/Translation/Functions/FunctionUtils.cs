using System.Text.RegularExpressions;

namespace DigitalLinkToolkit.Compression.Functions;

public partial class FunctionUtils
{
    private static readonly Regex _regex = FunctionRegex();
    
    public static string Execute(string functionDefinition, Dictionary<string, string> parameters)
    {
        var match = _regex.Match(functionDefinition);
        var function = match.Groups["function"].Value;
        var arguments = match.Groups["params"].Value.Split(',');

        return function switch
        {
            "SUBSTR" => Substring(parameters, arguments),
            "CONCAT" => Concat(parameters, arguments),
            "LENGTH" => Length(parameters, arguments),
            // split URN and URL encoding if needed
            "URNENCODE" or "URLENCODE" => UrlEncode(parameters, arguments),
            "URNDECODE" or "URLDECODE" => UrlDecode(parameters, arguments),
            "GS1CHECKSUM" => Checksum(parameters, arguments),
            _ => throw new ArgumentException("Invalid function definition.")
        };
    }

    private static string Substring(Dictionary<string, string> parameters, string[] arguments)
    {
        if (!parameters.TryGetValue(arguments[0], out var parameter))
        {
            throw new Exception($"Field {arguments[0]} not found.");
        }

        if (arguments.Length == 2)
        {
            var startIndex = ParseParameter(arguments[1], parameters);

            return parameter[startIndex..];
        }
        if (arguments.Length == 3)
        {
            var startIndex = ParseParameter(arguments[1], parameters);
            var endIndex = ParseParameter(arguments[2], parameters);

            return parameter.Substring(startIndex, endIndex);
        }

        throw new Exception("Invalid number of arguments for SUBSTRING function.");
    }

    private static string Concat(Dictionary<string, string> parameters, string[] arguments)
    {
        return string.Concat(arguments.Select(p => parameters[p]));
    }

    private static string Length(Dictionary<string, string> parameters, string[] arguments)
    {
        return parameters[arguments[0]].Length.ToString();
    }

    public static string Checksum(Dictionary<string, string> parameters, string[] arguments)
    {
        var value = parameters[arguments[0]];
        var weightedSum = 0;

        for (var i = 0; i < value.Length; i++)
        {
            var weight = i % 2 == 0 ? 3 : 1;
            weightedSum += (value[i] - '0') * weight;
        }

        var checkDigit = 10 - weightedSum % 10;

        return $"{checkDigit % 10}";
    }

    public static string UrlEncode(Dictionary<string, string> parameters, string[] arguments)
    {
        return Uri.EscapeDataString(parameters[arguments[0]]);
    }

    public static string UrlDecode(Dictionary<string, string> parameters, string[] arguments)
    {
        return Uri.UnescapeDataString(parameters[arguments[0]]);
    }

    private static int ParseParameter(string value, Dictionary<string, string> parameters)
    {
        if (int.TryParse(value, out var intValue))
        {
            return intValue;
        }
        else
        {
            return int.Parse(parameters[value]);
        }
    }

    [GeneratedRegex("^(?<function>[a-zA-Z0-9]+)\\((?<params>[a-zA-Z0-9,]+)\\)$")]
    private static partial Regex FunctionRegex();
}
