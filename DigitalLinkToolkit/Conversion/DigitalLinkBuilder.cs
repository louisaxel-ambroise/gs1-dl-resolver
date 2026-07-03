using DigitalLinkToolkit.Conversion.DTOs;
using DigitalLinkToolkit.Conversion.Model;
using DigitalLinkToolkit.Conversion.Validation;
using DigitalLinkToolkit.Exceptions;

namespace DigitalLinkToolkit.Conversion;

internal sealed class DigitalLinkBuilder(string hostUrl)
{
    public bool HasErrors => _issues.Count != 0;
    public bool HasPrimaryKey => _parsedAIs.Any(ai => ai.Key.Type is AIType.PrimaryKey);
    public IReadOnlyCollection<ValidationIssue> Issues => _issues.AsReadOnly();
    public IReadOnlyCollection<KeyValue> AIs => _parsedAIs.AsReadOnly();
    public IReadOnlyCollection<KeyValuePair<string, string?>> QueryString => _queryString.AsReadOnly();

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

            if (keyValue.Key.Type is AIType.PrimaryKey)
            {
                var gcpComponent = keyValue.Key.Components[0];

                if (gcpComponent is null || !gcpComponent.Flags.HasFlag(ComponentFlag.GCP))
                {
                    RegisterError(ErrorCodes.NoCompanyPrefix, "AI does not contain component with GCP", keyValue.Key.Code, keyValue.Value);
                }
                else if (!keyValue.Issues.Any())
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

    public void RegisterQueryString(string key, string? value)
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