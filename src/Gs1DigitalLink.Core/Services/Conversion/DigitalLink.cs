using Gs1DigitalLink.Core.Services.Conversion.Utils;
using System.Text;

namespace Gs1DigitalLink.Core.Services.Conversion;

public class DigitalLink
{
    public required string CompanyPrefix { get; init; }
    public required IEnumerable<KeyValue> AIs { get; init; }
    public required IEnumerable<KeyValuePair<string, string>> QueryString { get; init; }

    public string ToShortString()
    {
        var pathBuilder = new StringBuilder();

        foreach (var ai in AIs)
        {
            switch (ai.Key.Type)
            {
                case AIType.PrimaryKey:
                case AIType.Qualifier:
                    pathBuilder.Append(ai.Code).Append('/').Append(Uri.EscapeDataString(ai.Value)).Append('/');
                    break;
            }
        }

        pathBuilder = pathBuilder.Remove(pathBuilder.Length - 1, 1);

        return pathBuilder.ToString();
    }
}

public sealed record KeyValue
{
    public required Utils.Identifier Key { get; init; }
    public required string Value { get; init; }
    public required IEnumerable<ValidationIssue> Issues { get; init; }
    public string Code => Key.Code;
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
