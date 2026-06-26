using Goto.Data.Enums;

namespace Goto.Data.Entities;

public sealed class Anchor
{
    public int Id { get; set; }
    public required string Prefix { get; set; }
    public required string CompanyPrefix { get; set; }
    public string? Description { get; set; }
    public List<AnchorLink> Links { get; set; } = [];

    public List<AnchorLink> FindBestMatches(Language[] languages, string[] mediaTypes)
    {
        var candidates = languages
            .Select(lang => FindBestMatch(Links, link => link.MatchesLanguage(lang)))
            .FirstOrDefault(list => list.Count > 0, Links);

        candidates = mediaTypes
            .Select(type => FindBestMatch(candidates, link => link.MatchesMediaType(type)))
            .FirstOrDefault(list => list.Count > 0, candidates);

        return candidates;
    }

    private static List<AnchorLink> FindBestMatch(List<AnchorLink> links, Func<AnchorLink, Match> selector)
    {
        return links.GroupBy(selector)
            .OrderByDescending(grp => grp.Key)
            .Where(grp => grp.Key is not Match.NoMatch)
            .Select(grp => grp.ToList())
            .FirstOrDefault(EmptyList);
    }

    private static readonly List<AnchorLink> EmptyList = [];
}
