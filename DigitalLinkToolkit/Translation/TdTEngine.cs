using DigitalLinkToolkit.Compression.Functions;
using DigitalLinkToolkit.Translation.Functions;
using DigitalLinkToolkit.Translation.Model.EPCs;
using DigitalLinkToolkit.Translation.Model.Tables;
using System.Data;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using LevelProcessor = (DigitalLinkToolkit.Translation.Model.EPCs.Scheme Scheme, System.Collections.Generic.IEnumerable<DigitalLinkToolkit.Translation.Model.EPCs.Level> Levels);
using OptionProcessor = (DigitalLinkToolkit.Translation.Model.EPCs.Scheme Scheme, DigitalLinkToolkit.Translation.Model.EPCs.Level Level, DigitalLinkToolkit.Translation.Model.EPCs.Option Option, string PreProcessedInput);

namespace DigitalLinkToolkit.Translation;

public sealed class TdTEngine(List<Scheme> schemes, List<Table> tables)
{
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

        return Translate(decompressed.ToString(), string.Concat("uriStem=", host), LevelType.Gs1_Digital_Link.ToString());
    }

    public string Compress(string value, string host)
    {
        var binaryRepresentation = Translate(value, "dataToggle=1;filter=1", LevelType.Binary.ToString());

        // TODO: format binary using ex or eh
        return string.Concat(host, '/', binaryRepresentation);
    }

    public string Translate(string input, string parameterList, string outputFormat)
    {
        var parameters = ParseParameterList(parameterList); // 1. Setup
        var candidates = FindCandidateSchemes(input, parameters); // 2. Determine the EPC scheme and input format level.  
        var inputOption = FindInputOption(input, candidates, parameters); // 3. Determine the option that matches the input value
        var outputOption = FindOutputOption(inputOption.Scheme, inputOption.Option.OptionKey, outputFormat); // 6. Find the corresponding option in the output format 

        ExtractFieldValues(parameters, inputOption); // 4. Parse the input value to extract values for each field within the option 
        ApplyRules(parameters, inputOption.Level, RuleType.Extract); // 5. Perform any rules of type EXTRACT within the input format option in order to calculate additional derived fields
        ApplyRules(parameters, outputOption.Level, RuleType.Format); // 7. Perform any rules of type FORMAT within the output format in order to calculate additional derived fields

        var result = GrammarFormatter.Format(outputOption.Level, outputOption.Option, parameters); // 8. Use the grammar string and substitutions from the associative array to build the output value

        return PostProcessOutput(result, outputOption.Level);
    }

    private static string PostProcessOutput(string result, Level level)
    {
        if(level.Type is LevelType.Gs1_Digital_Link)
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

            foreach (var keys in level.DigitalLinkToolkitKeyQualifiers)
            {
                var v = kv.SingleOrDefault(x => x.Item1 == keys);

                if(v.Item1 is not null)
                {
                    scheme += v.Item1 + "/" + v.Item2 + "/";
                }
            }

            scheme = scheme.TrimEnd('/');
            var queryString = "?";

            foreach(var v in kv.Where(x => !level.DigitalLinkToolkitKeyQualifiers.Contains(x.Item1)))
            {
                queryString += v.Item1 + "=" + v.Item2 + "&";
            }

            if(queryString.Length > 1) scheme += queryString;

            return scheme;
        }

        return result;
    }

    private static (Scheme Scheme, Level Level, Option Option) FindOutputOption(Scheme scheme, string inputOptionKey, string outputFormat)
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
                if (!string.IsNullOrEmpty(rule.DecimalMinimum) && rule.DecimalMinimum.Any(c => c != '0'))
                {
                    if (BigInteger.Parse(result) < BigInteger.Parse(rule.DecimalMinimum))
                    {
                        throw new Exception($"Invalid value. Expected minimum of  {rule.DecimalMinimum}");
                    }
                }
                if (!string.IsNullOrEmpty(rule.DecimalMaximum) && (rule.DecimalMaximum.Length < rule.Length || rule.DecimalMaximum.Any(c => c != '9')))
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

            ValidateRange(field, value);
            if (!string.IsNullOrEmpty(field.DecimalMinimum) && field.DecimalMinimum.Any(c => c != '0'))
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
        if (level.Type == LevelType.Gs1_Digital_Link && inputOption.AISequence.Any())
        {
            var url = new Uri(input);
            var scheme = url.Scheme + "://" + url.Host;
            var urlPath = url.AbsolutePath.Trim('/').Split('/').Reverse();
            var queryElements = new List<KeyValuePair<string, string?>>();
            var pathElements = new List<KeyValuePair<string, string?>>();

            for (var i = 0; i < urlPath.Count(); i += 2)
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

            if (queryElements.Count > 0)
            {
                scheme += "?" + string.Join('&', queryElements.Select(p => p.Key + "=" + p.Value));
            }

            return scheme;
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

    private static void ValidateRange(Field field, string value)
    {
        if(field.DecimalMaximum is null && field.DecimalMinimum is null) return;
        
        var decimalValue = BigInteger.Parse(value);

        if(field.DecimalMinimum is not null && field.DecimalMinimum.Any(c => c is not '0') && decimalValue < BigInteger.Parse(field.DecimalMinimum))
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

        var parsedValue = value.ToBinaryValue();

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
}
