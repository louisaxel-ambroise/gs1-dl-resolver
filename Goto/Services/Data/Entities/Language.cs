using Goto.Services.Data.Enums;
using System.Globalization;

namespace Goto.Services.Data.Entities;

public sealed class Language
{
    public string Country { get; }
    public string? Region { get; }

    public Language(string value)
    {
        var parts = value.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.TrimEntries);

        if (parts.Length > 2 || parts.Any(p => p.Length is < 2 or > 3 || p.Any(c => !char.IsLetterOrDigit(c))))
        {
            throw new InvalidOperationException($"Invalid language: '{value}'. Shall use 2 or 3 letter country and region (optional)");
        }

        Country = parts[0];
        Region = parts.Length > 1 ? parts[1] : null;
    }

    public override string ToString()
    {
        return string.IsNullOrEmpty(Region) ? Country : string.Concat(Country, '-', Region);
    }

    internal Match Matches(Language other)
    {
        if(other.Country == Country)
        {
            if(other.Region == Region)
            {
                return Match.FullMatch;
            }
            if(string.IsNullOrEmpty(other.Region) || string.IsNullOrEmpty(Region))
            {
                return Match.WildcardMatch;
            }

            return Match.PartialMatch;
        }

        return Match.NoMatch;
    }

    public static Language Parse(string value)
    {
        var culture = new CultureInfo(value);

        return new Language(culture.Name);
    }
}
