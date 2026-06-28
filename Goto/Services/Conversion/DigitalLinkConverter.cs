using Goto.Infrastructure.Exceptions;
using Goto.Services.Conversion.Utils;
using Goto.Services.Conversion.Utils.Validation;
using System.Web;

namespace Goto.Services.Conversion;

internal sealed class DigitalLinkConverter(ApplicationIdentifiers identifiers) 
{
    public DigitalLink Parse(HttpRequest request)
    {
        var host = string.Concat(request.Scheme, "://", request.Host);

        return Parse(host, request.Path, request.QueryString.ToString());
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

        return TryParseUncompressedPath(parts, builder);
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

    #endregion

    private class DigitalLinkBuilder(string hostUrl)
    {
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
                    var gcpComponent = keyValue.Key.Components[0];

                    if (gcpComponent is null || !gcpComponent.Flags.HasFlag(ComponentFlag.GCP))
                    {
                        RegisterError(ErrorCodes.NoCompanyPrefix, "AI does not contain component with GCP", keyValue.Key.Code, keyValue.Value);
                    }
                    else if(!keyValue.Issues.Any())
                    {
                        var value = keyValue.Value[gcpComponent.Gcp..];
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

        public DigitalLink Build()
        {
            if (_issues.Count != 0)
            {
                throw new InvalidDigitalLinkException(_issues);
            }

            return new DigitalLink
            {
                HostUrl = hostUrl,
                CompanyPrefix = _companyPrefix,
                AIs = _parsedAIs.AsReadOnly(),
                QueryString = _queryString.AsReadOnly(),
            };
        }

        private readonly List<ValidationIssue> _issues = [];
        private readonly List<KeyValue> _parsedAIs = [];
        private readonly List<KeyValuePair<string, string?>> _queryString = [];
        private string _companyPrefix = string.Empty;
    }
}
