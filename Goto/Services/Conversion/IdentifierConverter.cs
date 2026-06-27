using Goto.Infrastructure.Exceptions;
using Goto.Services.Conversion.Utils;
using Goto.Services.Conversion.Utils.Validation;

namespace Goto.Services.Conversion;

public sealed class IdentifierConverter(ApplicationIdentifiers identifiers)
{
    public Identifier Parse(string input)
    {
        var parts = input.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var key = parts.Length > 0 ? identifiers.Identifiers.SingleOrDefault(i => i.Code == parts[0] && i.Type == AIType.PrimaryKey) : null;
        var companyPrefix = default(string?);

        if (parts.Length > 0 && key is null)
        {
            throw new InvalidDigitalLinkException([new() { Code = ErrorCodes.InvalidPrefix, Key = ErrorCodes.InvalidInput, Message = "Input is an invalid DigitalLink prefix", Value = input }]);
        }
        if (key is not null && !ValidateKey(key, parts[1], out companyPrefix))
        {
            throw new InvalidDigitalLinkException([new() { Code = ErrorCodes.InvalidCompanyPrefix, Key = ErrorCodes.InvalidCompanyPrefix, Message = "Input has an invalid company prefix", Value = input }]);
        }
        for (var i = 2; i < parts.Length - 2; i += 2)
        {
            if (!ValidateQualifier(parts[i], parts[i + 1]))
            {
                throw new InvalidDigitalLinkException([new() { Code = ErrorCodes.InvalidPrefix, Key = ErrorCodes.InvalidInput, Message = "Input is an invalid DigitalLink prefix", Value = input }]);
            }
        }

        return new()
        {
            CompanyPrefix = companyPrefix,
            Value = string.Join('/', parts)
        };
    }

    private bool ValidateQualifier(string code, string value)
    {
        var qualifier = identifiers.Identifiers.SingleOrDefault(i => i.Code == code && i.Type == AIType.PrimaryKey);

        if (qualifier is null) return false;
        if (value.Length > qualifier.Components.Sum(c => c.Length)) return false;

        return true;
    }

    private static bool ValidateKey(Utils.Identifier key, string value, out string? companyPrefix)
    {
        companyPrefix = null;
        if (value.Length > key.Components.Sum(c => c.Length)) return false;

        var gcpComponent = key.Components[0];
        var trimmedValue = value[gcpComponent.Gcp..];
        var gcpLength = CompanyPrefix.GetCompanyPrefixLength(trimmedValue);

        if(gcpLength < 0 || trimmedValue.Length < gcpLength) return false;

        companyPrefix = trimmedValue[..gcpLength];

        return true;
    }
}

public record Identifier
{
    public required string? CompanyPrefix { get; init; }
    public required string Value { get; init; }
}
