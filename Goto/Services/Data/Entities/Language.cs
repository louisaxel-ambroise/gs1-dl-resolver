using Goto.Services.Data.Enums;
using System.Globalization;

namespace Goto.Services.Data.Entities;

public sealed partial class Language
{
    public string Country { get; }
    public string? Region { get; }

    public Language(string value)
    {
        var parts = value.Split('-', 2, StringSplitOptions.TrimEntries | StringSplitOptions.TrimEntries);

        if (parts.Any(p => p.Length is < 2 or > 3 || p.Any(c => !char.IsLetterOrDigit(c))))
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

    public Match Matches(Language other)
    {
        if(other.Country == Country)
        {
            if(string.IsNullOrEmpty(other.Region) || string.IsNullOrEmpty(Region) || other.Region == Region)
            {
                return Match.FullMatch;
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
