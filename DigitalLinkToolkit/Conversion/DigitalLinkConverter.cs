using DigitalLinkToolkit.Conversion.DTOs;
using DigitalLinkToolkit.Conversion.Model;
using DigitalLinkToolkit.Conversion.Validation;
using DigitalLinkToolkit.Translation;
using DigitalLinkToolkit.Translation.Functions;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Web;

namespace DigitalLinkToolkit.Conversion;

public sealed class DigitalLinkConverter(OptimizationCodes optimizationCodes, ApplicationIdentifiers identifiers, TdTEngine translationEngine) 
{
    public DigitalLink Parse(HttpRequest request)
    {
        var host = string.Concat(request.Scheme, "://", request.Host);
        var path = request.Path.Value.TrimStart('/');
        var queryString = request.QueryString.ToString();

        if (path.StartsWith("eh") || path.StartsWith("ex"))
        {
            var decompressedDigitalLink = translationEngine.Decompress(path.TrimStart('/'), host);
            var decompressedUri = new Uri(decompressedDigitalLink);

            path = decompressedUri.AbsolutePath;
            queryString = decompressedUri.Query;
        }

        return Parse(host, path, queryString);
    }

    public DigitalLink Parse(string host, string path, string query)
    {
        var builder = new DigitalLinkBuilder(host);

        if (TryProcessUriPath(path.Trim('/'), builder))
        {
            ProcessQueryString(query, builder);
        }
        else
        {
            builder.RegisterError(ErrorCodes.InvalidInput, "Input string is not a valid DigitalLink URL");
        }

        return builder.Build();
    }

    #region DigitalLink parsing methods

    private void ProcessQueryString(string query, DigitalLinkBuilder builder)
    {
        var keyValuePair = HttpUtility.ParseQueryString(query);

        foreach (var key in keyValuePair.AllKeys.Where(x => !string.IsNullOrEmpty(x)))
        {
            var value = keyValuePair.Get(key) ?? string.Empty;

            if (identifiers.TryGet(key!, out var ai) && ai.Type is AIType.DataAttribute)
            {
                builder.RegisterAI(ComponentConverter.Parse(ai, value));
            }
            else
            {
                builder.RegisterQueryString(key!, value);
            }
        }
    }

    private bool TryProcessUriPath(string absolutePath, DigitalLinkBuilder builder)
    {
        var parts = absolutePath.Split('/');

        return TryParseCompressedPath(parts, builder) || TryParseUncompressedPath(parts, builder);
    }

    private bool TryParseUncompressedPath(string[] parts, DigitalLinkBuilder builder)
    {
        var ais = new List<KeyValue>();

        for (var i = 1; i < parts.Length; i += 2)
        {
            if (identifiers.TryGet(parts[^(i + 1)], out var ai) && ai.Type is AIType.PrimaryKey or AIType.Qualifier)
            {
                ais.Add(ComponentConverter.Parse(ai, parts[^i]));

                if (ai.Type is AIType.PrimaryKey) break;
            }
            else
            {
                return false;
            }
        }

        ais.Reverse();
        ais.ForEach(builder.RegisterAI);

        return ais.Count > 0 && ais[0].Key.Type is AIType.PrimaryKey;
    }

    private bool TryParseCompressedPath(string[] parts, DigitalLinkBuilder builder)
    {
        if (parts[^1].IsUriSafeBase64() && TryDecompress(parts[^1], out var decompressedValue))
        {
            // Review if logic can be simplified
            if (!decompressedValue.HasPrimaryKey)
            {
                if (parts.Length >= 3 && identifiers.TryGet(parts[^3], out var key) && key.Type is AIType.PrimaryKey)
                {
                    builder.RegisterAI(ComponentConverter.Parse(key, parts[^2]));
                }
                else
                {
                    return false;
                }
            }

            decompressedValue.AIs.ToList().ForEach(builder.RegisterAI);
            decompressedValue.QueryString.ToList().ForEach(keyValue => builder.RegisterQueryString(keyValue.Key, keyValue.Value));

            return true;
        }

        return false;
    }

    private bool TryDecompress(string compressedValue, out DigitalLinkBuilder result)
    {
        result = new("");
        var binaryStream = new Bitstream(string.Concat(compressedValue.Select(Alphabets.GetBinary)));

        while (binaryStream.Buffer(8))
        {
            if (binaryStream.Current[..4] == "1101")
            {
                if (binaryStream.Current[4..] != "0001")
                {
                    result.RegisterError(ErrorCodes.UnsupportedGS1Algorithm, "Specified GS1 algorithm is not supported");
                    return false;
                }
            }
            else if (binaryStream.Current[..4] == "1110")
            {
                result.RegisterError(ErrorCodes.UnsupportedProprietaryAlgorithm, "Specified Proprietary algorithm is not supported");
                return false;
            }
            else if (!ParseCompressedValue(binaryStream, result))
            {
                result.RegisterError(ErrorCodes.UnsupportedGS1Algorithm, "Specified GS1 algorithm is not supported");
                return false;
            }
        }

        return true;
    }

    private bool ParseCompressedValue(Bitstream binaryStream, DigitalLinkBuilder builder)
    {
        try
        {
            var ais = new List<string>();
            var current = Convert.ToByte(binaryStream.Current.ToString(), 2);
            var code = current.ToString("X2");

            if (code[0] == 'F')
            {
                ParseNonGS1KeyValuePairs(binaryStream, builder);
            }
            else if (!current.IsNumeric())
            {
                if (!optimizationCodes.TryGet(current.ToString("X2"), out var optimizedAis))
                {
                    builder.RegisterError(ErrorCodes.InvalidInput, "Input string is not a valid DigitalLink URL");
                    return false;
                }

                ais.AddRange(optimizedAis!.SequenceAIs);
            }
            else
            {
                if (!identifiers.CodeLength.TryGetValue(code, out var length))
                {
                    return false;
                }
                for (var i = 2; i < length; i++)
                {
                    binaryStream.Buffer(4);
                    var remain = Convert.ToByte(binaryStream.Current.ToString(), 2);

                    code += remain.ToNumericString();
                }

                ais.Add(code);
            }

            ais.ForEach(ai => ParseApplicationIdentifier(ai, binaryStream, builder));
        }
        catch
        {
            builder.RegisterError(ErrorCodes.InvalidCompressedValue, "Compressed value is not a valid DL");
        }

        return !builder.HasErrors;
    }

    private static void ParseNonGS1KeyValuePairs(Bitstream binaryStream, DigitalLinkBuilder builder)
    {
        var current = binaryStream.Current[4..].ToString();
        binaryStream.Buffer(3);
        current += binaryStream.Current.ToString();

        var keyLength = Convert.ToInt32(current, 2);
        var keyEncoding = Encodings.Values[3];
        var key = keyEncoding.Read(keyLength, binaryStream);

        var valueEncoding = GetEncoding(Charset.Alpha, binaryStream);
        binaryStream.Buffer(7);
        var valueLength = Convert.ToInt32(binaryStream.Current.ToString(), 2);
        var value = valueEncoding.Read(valueLength, binaryStream);

        builder.RegisterQueryString(key, value);
    }

    private void ParseApplicationIdentifier(string code, Bitstream inputStream, DigitalLinkBuilder builder)
    {
        if (identifiers.TryGet(code, out var ai))
        {
            var result = new List<Component>();
            var value = string.Empty;

            foreach (var component in ai.Components)
            {
                var encoding = GetEncoding(component.Type, inputStream);
                var length = GetBitsLength(component, inputStream);
                value += encoding.Read(length, inputStream);
            }

            builder.RegisterAI(ComponentConverter.Parse(ai, value));
        }
        else
        {
            builder.RegisterError(ErrorCodes.UnknownAI, "Unknown AI code", code);
        }
    }

    private static int GetBitsLength(AIComponent component, Bitstream stream)
    {
        if (component.Flags.HasFlag(ComponentFlag.FixedLength))
        {
            return component.Length;
        }
        else
        {
            var lengthBits = (int)Math.Ceiling(Math.Log(component.Length) / Math.Log(2));
            stream.Buffer(lengthBits);

            return Convert.ToInt32(stream.Current.ToString(), 2);
        }
    }

    private static Encodings GetEncoding(Charset charset, Bitstream stream)
    {
        static Encodings GetCharsetFromBuffer(Bitstream stream)
        {
            stream.Buffer(3);
            var encodingIndex = Convert.ToInt32(stream.Current.ToString(), 2);

            return Encodings.Values.ElementAt(encodingIndex);
        }

        return charset switch
        {
            Charset.Numeric => Encodings.Numeric,
            Charset.Alpha => GetCharsetFromBuffer(stream),
            _ => throw new Exception("Unknown charset")
        };
    }

    #endregion

    #region DigitalLink compression methods

    public string Compress(DigitalLink digitalLink)
    {
        var ais = digitalLink.AIs.AsEnumerable();
        var queryString = digitalLink.QueryString.AsEnumerable();
        var uncompressedQueryStrings = new List<string>();
        var resultBuffer = new StringBuilder();
        var compressedBuffer = new StringBuilder();

        if (optimizationCodes.TryGetMatching(ais.Select(x => x.Key.Code), out var optimization))
        {
            compressedBuffer.Append(Alphabets.GetAlphaBinary(optimization.Code));

            foreach (var element in optimization.SequenceAIs)
            {
                if (identifiers.TryGet(element, out var applicationIdentifier))
                {
                    var entry = ais.Single(a => a.Key.Code == element);
                    var remaining = entry.Value;

                    foreach (var component in entry.Key.Components)
                    {
                        var componentValue = component.GetValue(remaining);
                        remaining = remaining[componentValue.Length..];
                        compressedBuffer.Append(ComponentConverter.Format(component, componentValue));
                    }
                }
            }

            ais = ais.Where(x => !optimization.SequenceAIs.Contains(x.Key.Code));
        }

        foreach (var entry in ais)
        {
            compressedBuffer = entry.Key.Code.Aggregate(compressedBuffer, (b, c) => b.Append(Alphabets.GetAlphaBinary(c)));
            var remaining = entry.Value;

            foreach (var component in entry.Key.Components)
            {
                var componentValue = component.GetValue(remaining);
                remaining = remaining[componentValue.Length..];
                compressedBuffer.Append(ComponentConverter.Format(component, componentValue));
            }
        }

        foreach (var (key, value) in queryString)
        {
            uncompressedQueryStrings.Add($"{key}={Uri.EscapeDataString(value ?? string.Empty)}");
        }

        compressedBuffer.Append(new string('0', (6 - compressedBuffer.Length % 6) % 6));
        resultBuffer.Append(compressedBuffer.GetChars());

        if (uncompressedQueryStrings.Count > 0)
        {
            resultBuffer.Append('?').Append(string.Join('&', uncompressedQueryStrings));
        }

        return resultBuffer.ToString();
    }

    #endregion
}
