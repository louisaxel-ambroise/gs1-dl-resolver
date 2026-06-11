using System.Text.RegularExpressions;

namespace Goto.Data.Entities;

public sealed partial class Language
{
    public string Country { get; private set; }
    public string? Region { get; private set; }

    public Language(string value)
    {
        var match = Regex.Match(value);

        if (!match.Success)
            throw new InvalidOperationException($"Language is invalid: '{value}'");

        Country = match.Groups["country"].Value;
        Region = match.Groups["region"].Value;
    }

    public override string ToString()
    {
        return string.IsNullOrEmpty(Region) ? Country : string.Concat(Country, '-', Region);
    }

    [GeneratedRegex("^(?<country>[a-zA-Z]{2,3})(\\-(?<region>[a-zA-Z]{2,3}))?$")]
    public partial Regex Regex { get; }

    public Quality Matches(Language language)
    {
        if(language.Country == Country)
        {
            if(string.IsNullOrEmpty(language.Region) || string.IsNullOrEmpty(Region) || language.Region == Region)
            {
                return Quality.FullMatch;
            }

            return Quality.PartialMatch;
        }

        return Quality.NoMatch;
    }

    public override bool Equals(object? obj)
    {
        return obj is Language other
            && other.Country == Country 
            && other.Region == Region;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Country, Region);
    }
}
