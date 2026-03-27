using Gs1DigitalLink.Core.Services.Conversion.Utils.Validation;
using System.Text;
using System.Text.RegularExpressions;

namespace Gs1DigitalLink.Core.Services.Conversion.Utils;

internal static class ComponentConverter
{
    public static KeyValue Parse(Identifier identifier, string value)
    {
        var valueBuilder = new StringBuilder();
        var issues = new List<ValidationIssue>();
        var remaining = value;

        if (!Regex.IsMatch(value, $"^{identifier.Pattern}$"))
        {
            var validationIssue = new ValidationIssue
            {
                Code = ErrorCodes.InvalidAIValue,
                Message = "Value does not match the AI pattern",
                Key = identifier.Code,
                Value = value
            };

            issues.Add(validationIssue);
        }
        else
        {
            foreach (var component in identifier.Components)
            {
                if (component.Flags.HasFlag(ComponentFlag.GCP) && !CompanyPrefix.Validate(value[component.Gcp..]))
                {
                    var validationIssue = new ValidationIssue
                    {
                        Code = ErrorCodes.InvalidCompanyPrefix,
                        Message = "CompanyPrefix cound not be inferred from value",
                        Key = identifier.Code,
                        Value = value
                    };

                    issues.Add(validationIssue);
                }
                if (component.Flags.HasFlag(ComponentFlag.CheckDigit) && !CheckDigit.Validate(value))
                {
                    var validationIssue = new ValidationIssue
                    {
                        Code = ErrorCodes.InvalidCheckDigit,
                        Message = "CheckDigit validation failed",
                        Key = identifier.Code,
                        Value = value
                    };

                    issues.Add(validationIssue);
                }

                var componentValue = component.Flags.HasFlag(ComponentFlag.FixedLength)
                    ? remaining[..component.Length]
                    : remaining;

                remaining = remaining[..componentValue.Length];
                valueBuilder.Append(Uri.UnescapeDataString(componentValue));
            }
        }

        return new KeyValue
        {
            Key = identifier,
            Value = valueBuilder.ToString(),
            Issues = issues
        };
    }
}