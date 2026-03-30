using Gs1DigitalLink.Core.Contracts.Conversion;
using Gs1DigitalLink.Core.Model.Exceptions;
using Gs1DigitalLink.Core.Services.Conversion.Utils;
using Gs1DigitalLink.Core.Services.Conversion.Utils.Validation;

namespace Gs1DigitalLink.Core.Services.Conversion;

internal sealed class DigitalLinkPrefixConverter(ApplicationIdentifiers identifiers) : IDigitalLinkPrefixConverter
{
    public Identifier Parse(string input)
    {
        var parts = input.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var key = identifiers.Identifiers.SingleOrDefault(i => i.Code == parts[0] && i.Type == AIType.PrimaryKey);

        if (key is null || key == default)
        {
            throw new InvalidDigitalLinkException([new() { Code = ErrorCodes.InvalidPrefix, Key = ErrorCodes.InvalidInput, Message = "Input is an invalid prefix", Value = input }]);
        }
        if (parts.Length >= 2 && !ValidateKey(key, parts[1]))
        {
            throw new InvalidDigitalLinkException([new() { Code = ErrorCodes.InvalidPrefix, Key = ErrorCodes.InvalidInput, Message = "Input is an invalid prefix", Value = input }]);
        }
        for (var i=2; i<parts.Length-2; i += 2)
        {
            if (!ValidateQualifier(parts[i], parts[i + 1]))
            {
                throw new InvalidDigitalLinkException([new() { Code = ErrorCodes.InvalidPrefix, Key = ErrorCodes.InvalidInput, Message = "Input is an invalid prefix", Value = input }]);
            }
        }

        return new()
        {
            Value = input.Trim('/')
        };
    }

    private bool ValidateQualifier(string code, string value)
    {
        var qualifier = identifiers.Identifiers.SingleOrDefault(i => i.Code == code && i.Type == AIType.PrimaryKey);

        if (qualifier is null) return false;
        if (value.Length > qualifier.Components.Sum(c => c.Length)) return false;

        return true;
    }

    private static bool ValidateKey(Utils.Identifier key, string value)
    {
        if (value.Length > key.Components.Sum(c => c.Length)) return false;

        var gcpComponent = key.Components[0];
        var trimmedValue = value[gcpComponent.Gcp..];
        var gcpLength = CompanyPrefix.GetCompanyPrefixLength(trimmedValue);

        if(gcpLength < 0 || trimmedValue.Length < gcpLength) return false;

        return true;
    }
}

public record Identifier
{
    public required string Value { get; init; }
}
