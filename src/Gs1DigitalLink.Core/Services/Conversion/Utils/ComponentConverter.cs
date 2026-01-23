using Gs1DigitalLink.Core.Services.Conversion;
using Gs1DigitalLink.Core.Services.Conversion.Utils.Validation;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace Gs1DigitalLink.Core.Services.Conversion.Utils;

internal static class ComponentConverter
{
    public static string Format(AIComponent component, string value)
    {
        return component.Type switch
        {
            Charset.Numeric => FormatNumeric(component, value),
            Charset.Alpha => FormatAlpha(component, value),
            _ => throw new Exception("Unknown charset")
        };
    }

    private static string FormatNumeric(AIComponent component, string value)
    {
        if (component.Flags.HasFlag(ComponentFlag.FixedLength))
        {
            var componentValue = value[..component.Length];

            var c = BigInteger.Parse(componentValue, NumberStyles.Number).ToBinaryString();
            var expectedLength = (int)Math.Ceiling(component.Length * Math.Log(10) / Math.Log(2) + 0.01);

            return c.PadLeft(expectedLength, '0');
        }
        else
        {
            var c = Convert.ToString(Convert.ToInt32(value, 10), 2);
            var lengthSize = (int)Math.Ceiling(Math.Log(component.Length) / Math.Log(2) + 0.01);
            var l2 = Convert.ToString(value.Length, 2).PadLeft(lengthSize, '0');
            var nl = (int)Math.Ceiling(value.Length * Math.Log(10) / Math.Log(2) + 0.01);

            return l2 + c.PadLeft(nl, '0');
        }
    }

    private static string FormatAlpha(AIComponent component, string value)
    {
        var prefix = "";

        if (component.Flags.HasFlag(ComponentFlag.FixedLength))
        {
            value = value[..component.Length];
        }
        else
        {
            var nli = (int)Math.Ceiling(Math.Log(component.Length) / Math.Log(2) + 0.01);
            prefix = Convert.ToString(value.Length, 2).PadLeft(nli, '0');

            value = value[..value.Length];
        }

        return FormatHex(value, prefix);
    }

    private static string FormatHex(string componentValue, string? prefix = null)
    {
        if (componentValue.IsNumeric())
        {
            var nv = (int)Math.Ceiling(componentValue.Length * Math.Log(10) / Math.Log(2) + 0.01);

            return $"000{prefix}{BigInteger.Parse(componentValue, NumberStyles.Number).ToBinaryString().PadLeft(nv, '0')}";
        }
        else if (componentValue.IsLowerCaseHex())
        {
            return $"001{prefix}{Alphabets.GetAlphaBinary(componentValue)}";
        }
        else if (componentValue.IsUpperCaseHex())
        {
            return $"010{prefix}{Alphabets.GetAlphaBinary(componentValue)}";
        }
        else if (componentValue.IsUriSafeBase64())
        {
            return $"011{prefix}{Alphabets.GetBase64Binary(componentValue)}";
        }
        else
        {
            return $"100{prefix}{string.Concat(Encoding.ASCII.GetBytes(componentValue).Select(x => Convert.ToString(x, 2).PadLeft(7, '0')))}";
        }
    }

    public static KeyValue Parse(Identifier identifier, string value)
    {
        var components = new List<Component>();
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
                components.Add(new Component
                {
                    Definition = component,
                    Value = Uri.UnescapeDataString(componentValue)
                });
            }
        }

        return new KeyValue
        {
            Key = identifier,
            Components = components,
            Issues = issues
        };
    }
}