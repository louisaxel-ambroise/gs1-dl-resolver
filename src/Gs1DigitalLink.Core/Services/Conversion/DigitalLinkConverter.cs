using Gs1DigitalLink.Core.Services.Conversion.Utils;
using Gs1DigitalLink.Core.Services.Conversion.Utils.Validation;
using System.Text;
using System.Web;

namespace Gs1DigitalLink.Core.Services.Conversion;

public interface IDigitalLinkConverter
{
    CompressionResult Compress(string digitalLink, DigitalLinkCompressionOptions options);
    DigitalLink Parse(string digitalLink);
}

internal sealed class DigitalLinkConverter(OptimizationCodes optimizationCodes, ApplicationIdentifiers identifiers) : IDigitalLinkConverter
{
    public CompressionResult Compress(string input, DigitalLinkCompressionOptions options)
    {
        var digitalLink = Parse(input);
        var compressedResult = Compress(digitalLink, options);

        return new CompressionResult
        {
            UncompressedValue = digitalLink.ToString(),
            CompressedValue = compressedResult
        };
    }

    public DigitalLink Parse(string digitalLink)
    {
        var builder = new DigitalLinkBuilder();
        var splittedLink = digitalLink.Split('?', 2);

        var query = splittedLink.Length > 1 ? splittedLink.Last() : string.Empty;

        if (TryProcessUriPath(splittedLink.First().Trim('/'), builder))
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
        builder.SetType(DigitalLinkType.Uncompressed);

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
        builder.SetType(DigitalLinkType.FullyCompressed);

        if (parts[^1].IsUriSafeBase64() && TryDecompress(parts[^1], out var decompressedValue))
        {
            // Review if logic can be simplified
            if (!decompressedValue.HasPrimaryKey)
            {
                if (parts.Length >= 3 && identifiers.TryGet(parts[^3], out var key) && key.Type is AIType.PrimaryKey)
                {
                    builder.RegisterAI(ComponentConverter.Parse(key, parts[^2]));
                    builder.SetType(DigitalLinkType.PartiallyCompressed);
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
        result = new();
        var binaryStream = new BitStream(compressedValue);

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

    private bool ParseCompressedValue(BitStream binaryStream, DigitalLinkBuilder builder)
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

    private static void ParseNonGS1KeyValuePairs(BitStream binaryStream, DigitalLinkBuilder builder)
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

    private void ParseApplicationIdentifier(string code, BitStream inputStream, DigitalLinkBuilder builder)
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

    private static int GetBitsLength(AIComponent component, BitStream stream)
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

    private static Encodings GetEncoding(Charset charset, BitStream stream)
    {
        static Encodings GetCharsetFromBuffer(BitStream stream)
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

    private string Compress(DigitalLink digitalLink, DigitalLinkCompressionOptions options)
    {
        var ais = digitalLink.AIs.AsEnumerable();
        var queryString = digitalLink.QueryString.AsEnumerable();
        var uncompressedQueryStrings = new List<string>();
        var resultBuffer = new StringBuilder();
        var compressedBuffer = new StringBuilder();

        if (options.CompressionType is DLCompressionType.Partial)
        {
            var key = ais.Single(x => x.Key.Type is AIType.PrimaryKey);
            resultBuffer.Append(key.Code).Append('/').Append(key.Value);

            ais = ais.Except([key]);

            if (ais.Any(ai => ai.Key.Type != AIType.DataAttribute))
            {
                resultBuffer.Append('/');
            }
        }
        else if (options.CompressionType is DLCompressionType.Full)
        {
            if (optimizationCodes.TryGetMatching(ais.Select(x => x.Key.Code), out var optimization))
            {
                compressedBuffer.Append(Alphabets.GetAlphaBinary(optimization.Code));

                foreach (var element in optimization.SequenceAIs)
                {
                    if (identifiers.TryGet(element, out var applicationIdentifier))
                    {
                        var entry = ais.Single(a => a.Key.Code == element);

                        foreach (var component in entry.Components)
                        {
                            compressedBuffer.Append(ComponentConverter.Format(component.Definition, component.Value));
                        }
                    }
                }

                ais = ais.Where(x => !optimization.SequenceAIs.Contains(x.Key.Code));
            }
        }

        foreach (var entry in ais)
        {
            compressedBuffer = entry.Key.Code.Aggregate(compressedBuffer, (b, c) => b.Append(Alphabets.GetAlphaBinary(c)));

            foreach (var component in entry.Components)
            {
                compressedBuffer.Append(ComponentConverter.Format(component.Definition, component.Value));
            }
        }

        foreach(var (key, value) in queryString)
        {
            if (options.CompressQueryString && key.Length < 128 && key.IsUriSafeBase64())
            {
                var keyLength = Convert.ToString(key.Length, 2).PadLeft(7, '0');

                compressedBuffer.Append("1111").Append(keyLength).Append(Alphabets.GetBase64Binary(key ?? string.Empty));
                compressedBuffer.Append(ComponentConverter.Format(QueryStringComponent, Uri.EscapeDataString(value)));
            }
            else
            {
                uncompressedQueryStrings.Add($"{key}={Uri.EscapeDataString(value)}");
            }
        }

        compressedBuffer.Append(new string('0', (6 - compressedBuffer.Length % 6) % 6));
        resultBuffer.Append(compressedBuffer.GetChars());

        if(uncompressedQueryStrings.Count > 0)
        {
            resultBuffer.Append('?').Append(string.Join('&', uncompressedQueryStrings));
        }

        return resultBuffer.ToString();
    }

    private static readonly AIComponent QueryStringComponent = new()
    {
        Length = 127,
        Type = Charset.Alpha
    };

    #endregion

    private class DigitalLinkBuilder
    {
        public bool HasErrors => _issues.Count != 0;
        public bool HasPrimaryKey => _parsedAIs.Any(ai => ai.Key.Type is AIType.PrimaryKey);
        public IReadOnlyCollection<ValidationIssue> Issues => _issues.AsReadOnly();
        public IReadOnlyCollection<KeyValue> AIs => _parsedAIs.AsReadOnly();
        public IReadOnlyCollection<KeyValuePair<string, string>> QueryString => _queryString.AsReadOnly();

        public void RegisterAI(KeyValue keyValue)
        {
            var existingAI = _parsedAIs.SingleOrDefault(ai => ai.Key == keyValue.Key);

            if (existingAI is not null && existingAI.Value != keyValue.Value)
            {
                RegisterError(ErrorCodes.DuplicateAI, "Duplicate AI found", keyValue.Key.Code, keyValue.Value);
            }
            else if (existingAI is null)
            {
                _parsedAIs.Add(keyValue);
                _issues.AddRange(keyValue.Issues);

                if(keyValue.Key.Type is AIType.PrimaryKey)
                {
                    var gcpComponent = keyValue.Components.FirstOrDefault(c => c.Definition.Flags.HasFlag(ComponentFlag.GCP));

                    if (gcpComponent is null)
                    {
                        RegisterError(ErrorCodes.InvalidAIValue, "AI does not contain component with GCP", keyValue.Key.Code, keyValue.Value);
                    }
                    else 
                    {
                        var value = gcpComponent.Value[gcpComponent.Definition.Gcp..];
                        var gcpLength = CompanyPrefix.GetCompanyPrefixLength(value);

                        if (gcpLength > 0)
                        {
                            _companyPrefix = gcpLength > 0 && value.Length >= gcpLength ? value[..gcpLength] : string.Empty;
                        }
                    }
                }
            }
        }

        public void RegisterQueryString(string key, string value)
        {
            _queryString.Add(new(key, value));
        }

        public void RegisterError(string code, string message, string? ai = null, string? value = null)
        {
            var validationIssue = new ValidationIssue
            {
                Code = code,
                Message = message,
                Key = ai,
                Value = value
            };

            _issues.Add(validationIssue);
        }

        public void SetType(DigitalLinkType type)
        {
            _type = type;
        }

        public DigitalLink Build()
        {
            if (HasErrors)
            {
                throw new InvalidDigitalLinkException(Issues);
            }

            return new DigitalLink
            {
                CompanyPrefix = _companyPrefix,
                AIs = _parsedAIs.AsReadOnly(),
                QueryString = _queryString.AsReadOnly(),
                Type = _type
            };
        }

        private readonly List<ValidationIssue> _issues = [];
        private readonly List<KeyValue> _parsedAIs = [];
        private readonly List<KeyValuePair<string, string>> _queryString = [];
        private DigitalLinkType _type = DigitalLinkType.Unknown;
        private string _companyPrefix = string.Empty;
    }
}
