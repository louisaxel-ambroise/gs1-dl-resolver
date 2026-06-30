using FasTnT.TagDataTranslation.Functions;
using FasTnT.TagDataTranslation.Model.EPCs;
using FasTnT.TagDataTranslation.Model.Tables;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FasTnT.TagDataTranslation;

public class TdtEngineBuilder
{
    private readonly List<DefinitionFile> _definitions = [];
    private readonly List<Table> _tables = [];

    public TdtEngineBuilder AddDefinitionFile(string definitionFilePath) => AddDefinitionFile(File.OpenRead(definitionFilePath));

    private TdtEngineBuilder AddDefinitionFile(FileStream fileStream)
    {
        var deserialized = JsonSerializer.Deserialize<DefinitionFile>(fileStream) ?? throw new InvalidOperationException("Invalid definition file");
        _definitions.Add(deserialized);

        return this;
    }

    public TdtEngineBuilder AddTableFile(string tableFilePath) => AddTableFile(File.OpenRead(tableFilePath));

    private TdtEngineBuilder AddTableFile(FileStream fileStream)
    {
        var deserialized = JsonSerializer.Deserialize<Table>(fileStream) ?? throw new InvalidOperationException("Invalid definition file");
        _tables.Add(deserialized);

        return this;
    }

    public TdTEngine BuildEngine() => new(_definitions, _tables);
}

public sealed class TdTEngine(List<DefinitionFile> definitions, List<Table> tables)
{
    public string Decompress(string input)
    {
        if (Uri.TryCreate(input, new UriCreationOptions { DangerousDisablePathAndQueryCanonicalization = false }, out var result))
        {
            if (result.AbsolutePath.Trim("/").StartsWith("eh"))
            {
                var decompressed = new StringBuilder();

                foreach (var c in result.AbsolutePath.Trim('/')[2..])
                {
                    decompressed.Append(Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0'));
                }

                return decompressed.ToString();
            }
            if (result.AbsolutePath.Trim("/").StartsWith("ex"))
            {
                var decompressed = new StringBuilder();
                foreach (var c in result.AbsolutePath.Trim('/')[2..])
                {
                    decompressed.Append(Alphabets.GetBinary(c));
                }

                return decompressed.ToString();
            }
        }

        return input;
    }

    public string Compress(string input)
    {
        if (Uri.TryCreate(input, new UriCreationOptions { DangerousDisablePathAndQueryCanonicalization = false }, out var result))
        {
            var binary = Translate(input, "uriStem=https://id.goto.it.com;filter=0;dataToggle=1", LevelType.Binary.ToString());

            return binary;
        }

        return input;
    }

    public string Translate(string input, string parameterList, string outputFormat)
    {
        // 1. Setup
        var parameters = ParseParameterList(parameterList);

        // 2. Determine the EPC scheme and input format level.  
        var schemes = FindCandidateSchemes(input, parameters);

        // 3. Determine the option that matches the input value
        var inputOption = FindInputOption(input, schemes, parameters);

        input = PreProcessInput(parameters, input, inputOption.Level, inputOption.Option);

        // 4. Parse the input value to extract values for each field within the option 
        ExtractFieldValues(parameters, input, inputOption.Level, inputOption.Option);

        // 5. Perform any rules of type EXTRACT within the input format option in order to calculate additional derived fields
        ApplyRules(parameters, inputOption.Level, RuleType.Extract);

        // 6. Find the corresponding option in the output format 
        var outputOption = FindOutputOption(inputOption.Scheme, inputOption.Option.OptionKey, outputFormat);

        // 7. Perform any rules of type FORMAT within the output format in order to calculate additional derived fields
        ApplyRules(parameters, outputOption.Item2, RuleType.Format);

        // 8. Use the grammar string and substitutions from the associative array to build the output value
        var result = GrammarFormatter.Format(outputOption.Item2, outputOption.Item3, parameters);

        return PostProcessOutput(result, outputOption.Item2);
    }

    private static string PostProcessOutput(string result, Level level)
    {
        if(level.Type == LevelType.Gs1_Digital_Link)
        {
            var url = new Uri(result);
            var scheme = url.Scheme + "://" + url.Host + "/";
            var paths = url.AbsolutePath.Trim('/').Split('/');
            var kv = new List<(string, string)>();

            scheme += paths[0] + "/" + paths[1] + "/";
            for(var i =2; i<paths.Length; i += 2)
            {
                kv.Add((paths[i], paths[i + 1]));
            }
            foreach(var qv in url.Query.Trim('?').Split('&', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                kv.Add((qv.Split('=')[0], qv.Split('=')[1]));
            }
            var queryElements = new List<KeyValuePair<string, string?>>();
            var pathElements = new List<KeyValuePair<string, string?>>();

            foreach (var keys in level.GS1DigitalLinkKeyQualifiers)
            {
                var v = kv.SingleOrDefault(x => x.Item1 == keys);

                if(v.Item1 is not null)
                {
                    scheme += v.Item1 + "/" + v.Item2 + "/";
                }
            }

            scheme = scheme.TrimEnd('/');
            var queryString = "?";

            foreach(var v in kv.Where(x => !level.GS1DigitalLinkKeyQualifiers.Contains(x.Item1)))
            {
                queryString += v.Item1 + "=" + v.Item2 + "&";
            }

            if(queryString.Length > 1) scheme += queryString;

            return scheme;
        }

        return result;
    }

    private static (Scheme, Level, Option) FindOutputOption(Scheme scheme, string inputOptionKey, string outputFormat)
    {
        var level = scheme.Levels.Single(l => l.Type.ToString().Equals(outputFormat, StringComparison.OrdinalIgnoreCase));

        return (scheme, level, level.Options.Single(o => o.OptionKey == inputOptionKey));
    }

    private static void ApplyRules(Dictionary<string, string> fields, Level level, RuleType ruleType)
    {
        foreach (var rule in level.Rules.Where(r => r.Type == ruleType).OrderBy(r => r.Seq))
        {
            var function = FunctionUtils.ParseFunction(rule.Function);
            var result = function.Invoke(fields);

            if (rule.Length.HasValue)
            {
                if (result.Length < rule.Length.Value)
                {
                    result = rule.PadDir == Direction.Left
                        ? result.PadLeft(rule.Length.Value, rule.PadChar.ElementAt(0))
                        : result.PadRight(rule.Length.Value, rule.PadChar.ElementAt(0));
                }
                if (result.Length > rule.Length.Value)
                {
                    throw new Exception($"Invalid length for field {rule.NewFieldName}. Expected length of {rule.Length.Value}");
                }
                if (!string.IsNullOrEmpty(rule.DecimalMinimum))
                {
                    if (BigInteger.Parse(result) < BigInteger.Parse(rule.DecimalMinimum))
                    {
                        throw new Exception($"Invalid value. Expected minimum of  {rule.DecimalMinimum}");
                    }
                }
                if (!string.IsNullOrEmpty(rule.DecimalMaximum))
                {
                    if (BigInteger.Parse(result) > BigInteger.Parse(rule.DecimalMaximum))
                    {
                        throw new Exception($"Invalid value. Expected minimum of  {rule.DecimalMaximum}");
                    }
                }
                if (!Regex.IsMatch(result, $"${rule.CharacterSet}$"))
                {
                    throw new Exception($"Invalid value. Expected matching character set of {rule.CharacterSet}");
                }
            }

            fields[rule.NewFieldName] = result;
        }
    }

    private void ExtractFieldValues(Dictionary<string, string> parameters, string input, Level level, Option inputOption)
    {
        var match = Regex.Match(input, inputOption.Pattern);
        var groups = match.Groups;

        foreach (var field in inputOption.Fields)
        {
            var value = groups.Values.ElementAt(field.Seq).Value;

            if (!string.IsNullOrEmpty(field.CharacterSet))
            {
                if (!Regex.IsMatch(value, $"^{field.CharacterSet}$"))
                {
                    throw new Exception($"Invalid value. Expected {field.CharacterSet}");
                }
            }
            if (level.Type is LevelType.Binary)
            {
                value = ParseBinaryField(field, value);
            }

            ValidateRange(field, value);
            if (!string.IsNullOrEmpty(field.DecimalMinimum))
            {
                if (BigInteger.Parse(value) < BigInteger.Parse(field.DecimalMinimum))
                {
                    throw new Exception($"Invalid value. Expected minimum of  {field.DecimalMinimum}");
                }
            }
            if (!string.IsNullOrEmpty(field.DecimalMaximum))
            {
                if (BigInteger.Parse(value) > BigInteger.Parse(field.DecimalMaximum))
                {
                    throw new Exception($"Invalid value. Expected minimum of  {field.DecimalMaximum}");
                }
            }

            parameters.Add(field.Name, value);
        }

        if (level.Type == LevelType.Binary)
        {
            var remaining = input.Substring(match.Length);
            var bitstream = new Bitstream(remaining);

            if (inputOption.EncodedAIs.Any())
            {
                foreach (var encodedAI in inputOption.EncodedAIs.OrderBy(ai => ai.Seq))
                {
                    parameters.Add(encodedAI.Name, ParseEncodedAI(encodedAI, bitstream));
                }
            }
            if (parameters.TryGetValue("dataToggle", out var dataToggle) && dataToggle == "1")
            {
                while (TryParseAIDCData(bitstream, out var additionalData))
                {
                    parameters.Add("aidc+" + additionalData.Item1, additionalData.Item2);
                }
            }
        }
        if (level.Type == LevelType.Gs1_Digital_Link)
        {
            var data = input.IndexOf('?') > 0 ? input.Split('?', 2).Last() : "";

            foreach (var kv in data.Split('&', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Split('=', 2)))
            {
                parameters.Add("aidc+" + kv[0], kv.Length > 1 ? kv[1] : string.Empty);
            }
        }
        if (level.Type == LevelType.Gs1_AI_Json)
        {
            var data = JsonSerializer.Deserialize<IDictionary<string, string>>(input);

            foreach (var kv in data)
            {
                parameters.Add("aidc+" + kv.Key, kv.Value);
            }
        }
    }

    private static string PreProcessInput(Dictionary<string, string> parameters, string input, Level level, Option inputOption)
    {
        if(level.Type == LevelType.Gs1_Digital_Link && inputOption.AISequence.Any())
        {
            var url = new Uri(input);
            var scheme = url.Scheme + "://" + url.Host;
            var urlPath = url.AbsolutePath.Trim('/').Split('/').Reverse();
            var queryElements = new List<KeyValuePair<string, string?>>();
            var pathElements = new List<KeyValuePair<string, string?>>();

            for (var i = 0; i < urlPath.Count(); i+=2)
            {
                if (urlPath.ElementAt(i + 1) == inputOption.AISequence.First())
                {
                    pathElements.Insert(0, new(urlPath.ElementAt(i + 1), urlPath.ElementAt(i)));
                    break;
                }
                else if (inputOption.AISequence.Contains(urlPath.ElementAt(i + 1)))
                {
                    pathElements.Insert(0, new(urlPath.ElementAt(i + 1), urlPath.ElementAt(i)));
                }
                else
                {
                    queryElements.Add(new(urlPath.ElementAt(i + 1), urlPath.ElementAt(i)));
                }
            }

            scheme += "/" + string.Join('/', pathElements.Select(p => p.Key + "/" + p.Value));

            if (queryElements.Any())
            {
                scheme += "?" + string.Join('&', queryElements.Select(p => p.Key + "=" + p.Value));
            }

            return scheme;
        }
        if (level.Type == LevelType.Gs1_AI_Json)
        {
            var json = JsonSerializer.Deserialize<Dictionary<string, string>>(input) ?? [];
            var resultJson = new Dictionary<string, string>();
            var additionalValues = new Dictionary<string, string>();

            foreach (var (key, value) in json)
            {
                if(inputOption.AISequence.Contains(key))
                {
                    resultJson[key] = value;
                }
                else
                {
                    additionalValues["aidc+" + key] = value;
                }
            }

            return JsonSerializer.Serialize(resultJson);
        }

        return input;
    }

    private bool TryParseAIDCData(Bitstream bitstream, out (string, string) result)
    {
        result = ("", "");

        if (bitstream.Remaining >= 8)
        {
            var value = Convert.ToByte(bitstream.ReadUntil(8), 2);

            if(value >> 4 <= 9 && (value & 0x0F) <= 9)
            {
                var code = value.ToString("X2");
                var length = tables.Single(t => t.TableId == "K").Rows.Single(r => r.GetString("a") == code).GetNumber("b");

                for (var i = 2; i < length; i++)
                {
                    var encodedChar = bitstream.ReadUntil(4);
                    var remain = Convert.ToByte(encodedChar, 2);

                    code += remain.ToString("X1");
                }

                result.Item1 += code;

                var tableF = tables.Single(t => t.TableId == "F");
                var aiRow = tableF.Rows.Single(r => r.GetString("a") == code);

                result.Item2 = TagDataStandardFunctions.Parse(aiRow, bitstream);
                return true;
            }
        }

        return false;
    }

    private static void ValidateRange(Field field, string value)
    {
        if(field.DecimalMaximum is null && field.DecimalMinimum is null) return;
        
        var decimalValue = BigInteger.Parse(value);

        if(field.DecimalMinimum is not null && decimalValue < BigInteger.Parse(field.DecimalMinimum))
        {
            throw new Exception($"Invalid value. Expected minimum of {field.DecimalMinimum}");
        }
        if(field.DecimalMaximum is not null && decimalValue > BigInteger.Parse(field.DecimalMaximum))
        {
            throw new Exception($"Invalid value. Expected maximum of {field.DecimalMaximum}");
        }
    }

    private string ParseEncodedAI(EncodedAI encodedAI, Bitstream bitStream)
    {
        var tableF = tables.Single(t => t.TableId == "F");
        var aiRow = tableF.Rows.Single(r => r.GetString("a") == encodedAI.AI);

        return TagDataStandardFunctions.Parse(aiRow, bitStream);
    }

    private static string ParseBinaryField(Field field, string value)
    {
        if(field.BitPadDir is not null)
        {
            value = field.BitPadDir is Direction.Left
                ? value.TrimStart((field.PadChar ?? "0").ElementAt(0))
                : value.TrimEnd((field.PadChar ?? "0").ElementAt(0));
        }

        var parsedValue = Utils.ParseBinaryValue(value);

        if(field.Length is not null && parsedValue.Length < field.Length)
        {
            parsedValue = parsedValue.PadLeft(field.Length.Value, '0');
        }

        return parsedValue;
    }

    private static Dictionary<string, string> ParseParameterList(string parameterList)
    {
        var parameters = parameterList.Split(';', StringSplitOptions.RemoveEmptyEntries);

        return parameters.Select(p => p.Split('=', 2)).ToDictionary(kv => kv[0], kv => kv.Length > 1 ? kv[1] : string.Empty);
    }

    private IEnumerable<LevelProcessor> FindCandidateSchemes(string input, Dictionary<string, string> parameters)
    {
        var candidates = definitions.Select(d => d.TagDataTranslation.Scheme);

        foreach(var candidate in candidates)
        {
            var levels = candidate.Levels.Where(l => input.StartsWith(l.PrefixMatch));

            if (levels.Any())
            {
                yield return new LevelProcessor
                {
                    Scheme = candidate,
                    Levels = candidate.Levels.Where(l => input.StartsWith(l.PrefixMatch))
                };
            }
        }
    }

    private static OptionProcessor FindInputOption(string input, IEnumerable<LevelProcessor> schemes, Dictionary<string, string> parameters)
    {
        foreach(var candidate in schemes.OrderByDescending(s => s.Scheme.TagLength))
        {
            string optionKey;

            if (candidate.Scheme.OptionKey is null or "1")
            {
                optionKey = "1";
            }
            else if(!parameters.TryGetValue(candidate.Scheme.OptionKey, out optionKey))
            {
                continue;
            }
            
            foreach(var level in candidate.Levels)
            {
                foreach (var option in level.Options)
                {
                    var preProcessedInput = PreProcessInput(parameters, input, level, option);

                    if ((optionKey is null || option.OptionKey == optionKey) && Regex.IsMatch(preProcessedInput, option.Pattern))
                    {
                        return new OptionProcessor
                        {
                            Scheme = candidate.Scheme,
                            Level = level,
                            Option = option
                        };
                    }
                }
            }
        }

        throw new Exception("No matching option found");
    }
}

public sealed class Bitstream(string remaining)
{
    private int _position = 0;

    public int Remaining => remaining.Length - _position;
    public string RemainingStr => remaining.Substring(_position);

    public string ReadUntil(int bitNumber)
    {
        var toRead = remaining.Length - _position > bitNumber ? bitNumber : remaining.Length - _position;

        return Read(toRead);
    }

    public string Read(int bitNumber)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(_position + bitNumber, remaining.Length, nameof(bitNumber));
        _position += bitNumber;

        return remaining.Substring(_position-bitNumber, bitNumber);
    }
}

public static class Utils
{
    public static string ParseBinaryValue(string value)
    {
        return ParseBigInteger(value).ToString();
    }

    public static BigInteger ParseBigInteger(string value)
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