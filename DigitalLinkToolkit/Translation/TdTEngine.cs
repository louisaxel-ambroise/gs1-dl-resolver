using DigitalLinkToolkit.Compression.Functions;
using DigitalLinkToolkit.Translation.Functions;
using DigitalLinkToolkit.Translation.Model.EPCs;
using DigitalLinkToolkit.Translation.Model.Tables;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using LevelProcessor = (DigitalLinkToolkit.Translation.Model.EPCs.Scheme Scheme, System.Collections.Generic.IEnumerable<DigitalLinkToolkit.Translation.Model.EPCs.Level> Levels);
using OptionProcessor = (DigitalLinkToolkit.Translation.Model.EPCs.Scheme Scheme, DigitalLinkToolkit.Translation.Model.EPCs.Level Level, DigitalLinkToolkit.Translation.Model.EPCs.Option Option, string PreProcessedInput);

namespace DigitalLinkToolkit.Translation;

public sealed class TdTEngine(List<Scheme> schemes, List<Table> tables)
{
    private readonly GrammarFormatter _formatter = new(tables);

    public string Decompress(string path, string host)
    {
        var decompressed = new StringBuilder();

        if (path.StartsWith("eh"))
        {
            foreach (var c in path[2..])
            {
                decompressed.Append(Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0'));
            }
        }
        if (path.StartsWith("ex"))
        {
            foreach (var c in path[2..])
            {
                decompressed.Append(Alphabets.GetBinary(c));
            }
        }

        return Translate(decompressed.ToString(), string.Concat("uriStem=", host), LevelType.Gs1_Digital_Link);
    }

    public string Compress(string value)
    {
        var host = value[..value.IndexOf('/', 8)];
        var binaryRepresentation = Translate(value, "dataToggle=1;filter=1", LevelType.Binary);
        var expectedLength = (int)Math.Ceiling((double)binaryRepresentation.Length / 6) * 6;
        binaryRepresentation = binaryRepresentation.PadRight(expectedLength, '0');

        var result = new StringBuilder().Append(host).Append("/ex");

        for (var i = 0; i < binaryRepresentation.Length; i+=6)
        {
            result.Append(Alphabets.GetChar(binaryRepresentation.AsSpan(i, 6)));
        }

        return result.ToString();
    }

    public string Translate(string input, string parameterList, string outputFormat)
    {
        return Translate(input, parameterList, Enum.Parse<LevelType>(outputFormat, true));
    }

    public string Translate(string input, string parameterList, LevelType outputFormat)
    {
        var parameters = ParseParameterList(parameterList); // 1. Setup
        var candidates = FindCandidateSchemes(input, parameters); // 2. Determine the EPC scheme and input format level.  
        var inputOption = FindInputOption(input, candidates, parameters); // 3. Determine the option that matches the input value
        var outputOption = FindOutputOption(inputOption.Scheme, inputOption.Option.OptionKey, outputFormat); // 6. Find the corresponding option in the output format 

        ExtractFieldValues(parameters, inputOption); // 4. Parse the input value to extract values for each field within the option 
        ApplyRules(parameters, inputOption.Level, RuleType.Extract); // 5. Perform any rules of type EXTRACT within the input format option in order to calculate additional derived fields
        ApplyRules(parameters, outputOption.Level, RuleType.Format); // 7. Perform any rules of type FORMAT within the output format in order to calculate additional derived fields

        var result = _formatter.Format(outputOption.Level, outputOption.Option, parameters); // 8. Use the grammar string and substitutions from the associative array to build the output value

        return PostProcessOutput(result, outputOption.Level, parameters);
    }

    private static string PostProcessOutput(string result, Level level, Dictionary<string, string> parameters)
    {
        // TODO: append AIDC data to the end of the URL
        if (level.Type is LevelType.Gs1_Digital_Link && level.DigitalLinkToolkitKeyQualifiers.Any())
        {
            var parts = ParseUrl(result, out var pathStart);
            var path = parts[0] + "/" + parts[1];

            foreach(var qualifier in level.DigitalLinkToolkitKeyQualifiers)
            {
                if(parts.IndexOf(qualifier) is var index && index > 0 && index < parts.Length - 1)
                {
                    path += "/" + qualifier + "/" + parts[index+1];
                }
            }
            
            return string.Concat(result[..pathStart], '/', path);
        }

        return result;
    }

    private static (Scheme Scheme, Level Level, Option Option) FindOutputOption(Scheme scheme, string inputOptionKey, LevelType outputFormat)
    {
        var level = scheme.Levels.Single(l => l.Type == outputFormat);

        return (scheme, level, level.Options.Single(o => o.OptionKey == inputOptionKey));
    }

    private static void ApplyRules(Dictionary<string, string> parameters, Level level, RuleType ruleType)
    {
        foreach (var rule in level.Rules.Where(r => r.Type == ruleType).OrderBy(r => r.Seq))
        {
            var result = FunctionUtils.Execute(rule.Function, parameters);

            if (rule.Length.HasValue)
            {
                if (result.Length < rule.Length.Value && rule.PadChar is not null)
                {
                    result = rule.PadDir == Direction.Left
                        ? result.PadLeft(rule.Length.Value, rule.PadChar.ElementAt(0))
                        : result.PadRight(rule.Length.Value, rule.PadChar.ElementAt(0));
                }
                if (result.Length > rule.Length.Value)
                {
                    throw new Exception($"Invalid length for field {rule.NewFieldName}. Expected length of {rule.Length.Value}");
                }
                if (!string.IsNullOrEmpty(rule.DecimalMinimum) && !ValidationRule.MinValue(rule.DecimalMinimum, result))
                {
                    throw new Exception($"Invalid value. Expected minimum of {rule.DecimalMinimum}");
                }
                if (!string.IsNullOrEmpty(rule.DecimalMaximum) && !ValidationRule.MaxValue(rule.DecimalMaximum, result))
                {
                    throw new Exception($"Invalid value. Expected maximum of {rule.DecimalMaximum}");
                }
                if (!Regex.IsMatch(result, $"${rule.CharacterSet}$"))
                {
                    throw new Exception($"Invalid value. Expected matching character set of {rule.CharacterSet}");
                }
            }

            parameters[rule.NewFieldName] = result;
        }
    }

    private void ExtractFieldValues(Dictionary<string, string> parameters, OptionProcessor option)
    {
        var match = Regex.Match(option.PreProcessedInput, option.Option.Pattern);
        var groups = match.Groups;

        foreach (var field in option.Option.Fields)
        {
            var value = groups.Values.ElementAt(field.Seq).Value;

            if (!string.IsNullOrEmpty(field.CharacterSet) && !Regex.IsMatch(value, $"^{field.CharacterSet}$"))
            {
                throw new Exception($"Invalid value. Expected {field.CharacterSet}");
            }
            if (option.Level.Type is LevelType.Binary)
            {
                value = ParseBinaryField(field, value);
            }

            if (!string.IsNullOrEmpty(field.DecimalMinimum) && !ValidationRule.MinValue(field.DecimalMinimum, value))
            {
                throw new Exception($"Invalid value. Expected minimum of {field.DecimalMinimum}");
            }
            if (!string.IsNullOrEmpty(field.DecimalMaximum) && !ValidationRule.MaxValue(field.DecimalMaximum, value))
            {
                throw new Exception($"Invalid value. Expected maximum of {field.DecimalMaximum}");
            }

            parameters.Add(field.Name, value);
        }

        if (option.Level.Type == LevelType.Binary)
        {
            var remaining = option.PreProcessedInput[match.Length..];
            var bitstream = new Bitstream(remaining);

            if (option.Option.EncodedAIs.Any())
            {
                foreach (var encodedAI in option.Option.EncodedAIs.OrderBy(ai => ai.Seq))
                {
                    parameters.Add(encodedAI.Name, ParseEncodedAI(encodedAI, bitstream));
                }
            }
            if (parameters.TryGetValue("dataToggle", out var dataToggle) && dataToggle == "1")
            {
                while (TryParseAIDCData(bitstream, out var additionalData))
                {
                    parameters.Add("aidc+" + additionalData.Code, additionalData.Value);
                }
            }
        }
        else if (option.Level.Type == LevelType.Gs1_Digital_Link)
        {
            var data = option.PreProcessedInput.IndexOf('?') > 0 ? option.PreProcessedInput.Split('?', 2).Last() : "";

            foreach (var kv in data.Split('&', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Split('=', 2)))
            {
                parameters.Add("aidc+" + kv[0], kv.Length > 1 ? kv[1] : string.Empty);
            }
        }
        else if (option.Level.Type == LevelType.Gs1_AI_Json)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(option.PreProcessedInput) ?? [];

            foreach (var kv in data)
            {
                parameters.Add("aidc+" + kv.Key, kv.Value);
            }
        }
    }

    private static string PreProcessInput(string input, Level level, Option inputOption)
    {
        if (level.Type == LevelType.Gs1_Digital_Link && level.DigitalLinkToolkitKeyQualifiers.Any())
        {
            var parts = ParseUrl(input, out var pathStart);
            var path = parts[0] + "/" + parts[1];

            foreach (var qualifier in level.DigitalLinkToolkitKeyQualifiers)
            {
                if (parts.IndexOf(qualifier) is var index && index > 0 && index < parts.Length - 1)
                {
                    path += "/" + qualifier + "/" + parts[index + 1];
                }
            }

            var queryElements = Enumerable.Range(1, (parts.Length-1) / 2).Where(i => !level.DigitalLinkToolkitKeyQualifiers.Contains(parts[i * 2])).Select(i => $"{parts[i * 2]}={parts[i * 2 + 1]}");
            var url = string.Concat(input[..pathStart], '/', path);

            return queryElements.Any() 
                ? string.Concat(url, '?', string.Join('&', queryElements)) 
                : url;
        }
        else if (level.Type == LevelType.Gs1_AI_Json && inputOption.AISequence.Any())
        {
            var json = JsonSerializer.Deserialize<Dictionary<string, string>>(input) ?? [];
            var resultJson = new Dictionary<string, string>();
            var additionalValues = new Dictionary<string, string>();

            foreach (var (key, value) in json)
            {
                if (inputOption.AISequence.Contains(key))
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
        else
        {
            return input;
        }
    }

    private bool TryParseAIDCData(Bitstream bitstream, out (string Code, string Value) result)
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

                result.Code += code;

                var tableF = tables.Single(t => t.TableId == "F");
                var aiRow = tableF.Rows.Single(r => r.GetString("a") == code);

                result.Value = TagDataStandardFunctions.Parse(aiRow, bitstream);
                return true;
            }
        }

        return false;
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

        string parsedValue;

        if (field.Encoding is not null)
        {
            parsedValue = TagDataStandardFunctions.Parse(field.Encoding, new(value));
        }
        else
        {
            parsedValue = value.ToBinaryValue();
        }

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
        foreach(var candidate in schemes)
        {
            var levels = candidate.Levels
                .Where(l => input.StartsWith(l.PrefixMatch) && (l.RequiredParsingParameters is null || l.RequiredParsingParameters.Split(',').All(parameters.ContainsKey)));

            if(levels.Any())
            {
                yield return (candidate, levels);
            }
        }
    }

    private static OptionProcessor FindInputOption(string input, IEnumerable<LevelProcessor> schemes, Dictionary<string, string> parameters)
    {
        foreach(var candidate in schemes.OrderByDescending(s => s.Scheme.TagLength))
        {
            string? optionKey;

            if (candidate.Scheme.OptionKey is null or "1" or "dateType")
            {
                optionKey = null;
            }
            else if (!parameters.TryGetValue(candidate.Scheme.OptionKey, out optionKey))
            {
                continue;
            }
            
            foreach (var level in candidate.Levels)
            {
                foreach (var option in level.Options)
                {
                    var preProcessedInput = PreProcessInput(input, level, option);

                    if ((optionKey is null || option.OptionKey == optionKey) && Regex.IsMatch(preProcessedInput, option.Pattern))
                    {
                        return (candidate.Scheme, level, option, preProcessedInput);
                    }
                }
            }
        }

        throw new Exception("No matching option found");
    }

    private static string[] ParseUrl(string url, out int pathStart)
    {
        pathStart = url.IndexOf('/', 8); // 8 = LEN(https://)
        var queryStart = url.IndexOf('?', pathStart);
        var parts = default(IEnumerable<string>);

        if(queryStart > 0)
        {
            parts = url[pathStart..queryStart].Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Union(url[(queryStart+1)..].Split('&').SelectMany(s => s.Split('=')));
        }
        else
        {
            parts = url[pathStart..].Split('/', StringSplitOptions.RemoveEmptyEntries);
        }

        return [.. parts];
    }
}
