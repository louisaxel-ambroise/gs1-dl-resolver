using Goto.Services.Data.Enums;
using System.Net.Http.Headers;

namespace Goto.Services.Data.Entities;

public sealed class MediaType
{
    public const string Json = "application/json";
    public const string Html = "text/html";
    public const string Linkset = "application/linkset+json";

    public string Type { get; }
    public string SubType { get; }

    public MediaType(string value)
    {
        var parts = value.Split('/', 2, StringSplitOptions.TrimEntries | StringSplitOptions.TrimEntries);

        if (parts.Length != 2)
        {
            throw new InvalidOperationException($"Invalid media type: '{value}'. Shall use format 'type/subtype' (wildcard allowed)");
        }

        Type = parts[0];
        SubType = parts[1];
    }

    public Match Matches(MediaType other)
    {
        if (other.Type == Type)
        {
            if (other.SubType == Wildcard || SubType == Wildcard || other.SubType == SubType)
            {
                return Match.FullMatch;
            }

            return Match.PartialMatch;
        }
        if (Type == Wildcard || other.Type == Wildcard)
        {
            if (other.SubType == Wildcard || SubType == Wildcard || other.SubType == SubType)
            {
                return Match.PartialMatch;
            }
        }

        return Match.NoMatch;
    }

    public override string ToString()
    {
        return $"{Type}/{SubType}";
    }

    public static MediaType Parse(string value)
    {
        var headerValue = MediaTypeHeaderValue.Parse(value);

        return new MediaType(headerValue.MediaType ?? "*/*");
    }

    private const string Wildcard = "*";
}
