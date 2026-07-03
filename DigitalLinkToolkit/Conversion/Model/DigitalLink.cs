using DigitalLinkToolkit.Conversion.DTOs;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Tavis.UriTemplates;

namespace DigitalLinkToolkit.Conversion.Model;

public sealed class DigitalLink
{
    public required string HostUrl { get; init; }
    public required string CompanyPrefix { get; init; }
    public required IEnumerable<KeyValue> AIs { get; init; }
    public required IEnumerable<KeyValuePair<string, string?>> QueryString { get; init; }

    public override string ToString()
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

    public IReadOnlyList<string> GetPrefixValues()
    {
        var key = AIs.Single(ai => ai.Key.Type is AIType.PrimaryKey);
        var prefixes = new List<string>([string.Empty, key.Code]);

        var gcpPosition = key.Value.IndexOf(CompanyPrefix);
        var position = gcpPosition + CompanyPrefix.Length;

        prefixes.Add(string.Join("/", key.Code, key.Value[..position]));

        while (position < key.Value.Length)
        {
            prefixes.Add(string.Concat(prefixes.Last(), key.Value[position++]));
        }

        foreach (var qualifier in AIs.Where(ai => ai.Key.Type is AIType.Qualifier))
        {
            prefixes.Add(string.Join("/", prefixes.Last(), qualifier.Code));
            prefixes.Add(string.Join("/", prefixes.Last(), qualifier.Value));
        }

        return prefixes;
    }

    public string FormatUriTemplates(string link)
    {
        var parameters = GetDigitalLinkParameters();
        var template = new UriTemplate(link);

        template.AddParameters(parameters);

        return QueryHelpers.AddQueryString(template.Resolve(), QueryString.Where(qs => qs.Key != "linkType"));
    }

    private Dictionary<string, object> GetDigitalLinkParameters()
    {
        var parameters = new Dictionary<string, object>();

        foreach (var ai in AIs)
        {
            parameters[ai.Key.Code] = ai.Value;

            if (!string.IsNullOrEmpty(ai.Key.ShortCode))
            {
                parameters[ai.Key.ShortCode] = ai.Value;
            }
        }

        return parameters;
    }

    public string BuildLinksetLink()
    {
        var path = ToString();
        var query = AIs
            .Where(ai => ai.Key.Type is AIType.DataAttribute)
            .Select(ai => new KeyValuePair<string, string?>(ai.Key.Code, ai.Value))
            .Union(QueryString)
            .ToDictionary();

        query["linkType"] = "linkset";

        return string.Concat(HostUrl, '/', path, '?', string.Join('&', query.Select(kv => $"{kv.Key}={kv.Value}")));
    }
}

public sealed record KeyValue
{
    public required AIIdentifier Key { get; init; }
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
