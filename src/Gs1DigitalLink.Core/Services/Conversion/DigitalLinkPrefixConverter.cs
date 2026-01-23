using Gs1DigitalLink.Core.Services.Conversion.Utils;
using Gs1DigitalLink.Core.Services.Conversion.Utils.Validation;

namespace Gs1DigitalLink.Core.Services.Conversion;

public interface IDigitalLinkPrefixConverter
{
    Identifier Parse(string input);
}

internal class DigitalLinkPrefixConverter(ApplicationIdentifiers identifiers) : IDigitalLinkPrefixConverter
{
    public Identifier Parse(string input)
    {
        var parts = input.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var key = identifiers.Identifiers.SingleOrDefault(i => i.Code == parts[0] && i.Type == AIType.PrimaryKey);
        var companyPrefix = string.Empty;

        if (key is null)
        {
            throw new InvalidDigitalLinkException([new() { Code = "Invalid prefix", Key = ErrorCodes.InvalidInput, Message = "Input is an invalid prefix", Value = input }]);
        }
        if (parts.Length >= 2 && !ValidateKey(key, parts[1], out companyPrefix))
        {
            throw new InvalidDigitalLinkException([new() { Code = "Invalid prefix", Key = ErrorCodes.InvalidInput, Message = "Input is an invalid prefix", Value = input }]);
        }
        for (var i=2; i<parts.Length-2; i += 2)
        {
            if (!ValidateQualifier(parts[i], parts[i + 1]))
            {
                throw new InvalidDigitalLinkException([new() { Code = "Invalid prefix", Key = ErrorCodes.InvalidInput, Message = "Input is an invalid prefix", Value = input }]);
            }
        }

        return new()
        {
            CompanyPrefix = companyPrefix,
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

    private static bool ValidateKey(Utils.Identifier key, string value, out string companyPrefix)
    {
        companyPrefix = string.Empty;

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
    public required string Value { get; init; }
    public required string CompanyPrefix { get; set; }
}
