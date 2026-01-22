using Gs1DigitalLink.Core.Services.Conversion.Utils;

namespace Gs1DigitalLink.Core.Services.Conversion;

public record CompressionResult
{
    public required string UncompressedValue { get; init; }
    public required string CompressedValue { get; init; }
    public decimal CompressionRate => Math.Round(100 - (100*Convert.ToDecimal(CompressedValue.Length)/Convert.ToDecimal(UncompressedValue.Length)), 2);
}

public class DigitalLink
{
    public required string CompanyPrefix { get; init; }
    public required IEnumerable<KeyValue> AIs { get; init; }
    public required IEnumerable<KeyValuePair<string, string>> QueryString { get; init; }
    public required DigitalLinkType Type { get; init; }

    public override string ToString() => ToString(true);

    public string ToString(bool includeQueryString)
    {
        var pathBuilder = new List<string>();
        var queryBuilder = new List<string>();

        foreach (var ai in AIs)
        {
            switch (ai.Key.Type)
            {
                case AIType.DataAttribute:
                    queryBuilder.Add($"{Uri.EscapeDataString(ai.Code)}={Uri.EscapeDataString(ai.Value)}");
                    break;
                case AIType.PrimaryKey:
                case AIType.Qualifier:
                    pathBuilder.Add($"{ai.Code}/{Uri.EscapeDataString(ai.Value)}");
                    break;
            }
        }
        if (includeQueryString)
        {
            foreach (var queryString in QueryString)
            {
                queryBuilder.Add($"{Uri.EscapeDataString(queryString.Key)}={Uri.EscapeDataString(queryString.Value)}");
            }
        }

        var result = string.Join('/', pathBuilder);

        if (queryBuilder.Count > 0)
        {
            result += "?" + string.Join('&', queryBuilder);
        }

        return result;
    }
}

public sealed record KeyValue
{
    public required Utils.Identifier Key { get; init; }
    public required IEnumerable<Component> Components { get; init; }
    public required IEnumerable<ValidationIssue> Issues { get; init; }
    public string Code => Key.Code;
    public string Value => string.Concat(Components.Select(c => c.Value));
}

public sealed record ValidationIssue
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public required string? Key { get; init; }
    public required string? Value { get; init; }

}
public sealed record Component
{
    public required AIComponent Definition { get; init; }
    public required string Value { get; init; }
}

public enum DigitalLinkType
{
    Unknown,
    FullyCompressed,
    PartiallyCompressed,
    Uncompressed
}