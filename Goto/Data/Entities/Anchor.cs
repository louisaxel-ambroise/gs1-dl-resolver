using Goto.Data.Enums;

namespace Goto.Data.Entities;

public sealed class Anchor
{
    public int Id { get; set; }
    public required string Prefix { get; set; }
    public required string CompanyPrefix { get; set; }
    public required string Description { get; set; }
    public List<AnchorLink> Links { get; set; } = [];

    public List<AnchorLink> FindBestMatches(string linkType, Language[] languages, string[] mediaTypes)
    {
        var candidates = Links.Where(l => l.LinkType == linkType);

        return FindBestMatches(candidates, languages, mediaTypes);
    }

    public List<AnchorLink> FindBestMatches(Language[] languages, string[] mediaTypes)
    {
        var candidates = Links.Where(l => l.IsDefault);

        return FindBestMatches(candidates, languages, mediaTypes);
    }

    private static List<AnchorLink> FindBestMatches(IEnumerable<AnchorLink> candidates, Language[] languages, string[] mediaTypes)
    {
        candidates = candidates.OrderBy(l => l.Id);
        candidates = languages
            .Select(lang => FindBestMatch(candidates, link => link.MatchesLanguage(lang)))
            .FirstOrDefault(list => list.Any(), candidates);

        candidates = mediaTypes
            .Select(type => FindBestMatch(candidates, link => link.MatchesMediaType(type)))
            .FirstOrDefault(list => list.Any(), candidates);

        return [.. candidates];
    }

    private static IEnumerable<AnchorLink> FindBestMatch(IEnumerable<AnchorLink> links, Func<AnchorLink, Match> selector)
    {
        return links.GroupBy(selector)
            .OrderByDescending(grp => grp.Key)
            .Where(grp => grp.Key is not Match.NoMatch)
            .FirstOrDefault(EmptyList);
    }

    private static readonly IEnumerable<AnchorLink> EmptyList = [];
}
