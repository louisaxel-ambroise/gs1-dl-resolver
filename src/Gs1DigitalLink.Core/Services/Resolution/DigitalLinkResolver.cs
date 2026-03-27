using Gs1DigitalLink.Core.Model;
using Gs1DigitalLink.Core.Services.Conversion;
using Microsoft.EntityFrameworkCore;

namespace Gs1DigitalLink.Core.Services.Resolution;

public interface IDigitalLinkResolver
{
    IResolutionResult ResolveLinkSet(DigitalLink digitalLink, DateTimeOffset applicability);
    IResolutionResult ResolveLinkType(DigitalLink digitalLink, DateTimeOffset applicability, string? linkType);
}

internal sealed class DigitalLinkResolver(ResolverContext context, ILanguageContext languageContext) : IDigitalLinkResolver
{
    public IResolutionResult ResolveLinkSet(DigitalLink digitalLink, DateTimeOffset applicability)
    {
        var candidates = Resolve(digitalLink);

        return FormatLinkset(digitalLink, candidates, applicability);
    }

    public IResolutionResult ResolveLinkType(DigitalLink digitalLink, DateTimeOffset applicability, string? linkType)
    {
        var candidates = Resolve(digitalLink);
        var languages = languageContext.GetLanguages();

        if (linkType is null && candidates.Any(p => p.Links.Any(l => l.IsApplicableAt(applicability))))
        {
            var matching = candidates.OrderByDescending(p => p.Value.Length).First(p => p.Links.Any(l => l.IsApplicableAt(applicability)));

            if (matching.IsLinksetDefault)
            {
                return FormatLinkset(digitalLink, candidates, applicability);
            }
        }

        var linkTypes = string.IsNullOrEmpty(linkType) ? DefaultLinkTypes : [linkType];
        var matchingLinks = FindMatchingLinks(candidates, applicability, linkTypes);

        matchingLinks = FilterByLanguage(matchingLinks, languages);
        matchingLinks = digitalLink.FormatUriTemplates(matchingLinks);

        return new LinkTypeResult(matchingLinks);
    }

    private IEnumerable<Prefix> Resolve(DigitalLink digitalLink)
    {
        var prefixValues = digitalLink.GetPrefixValues();

        return context.Prefixes
            .AsNoTracking()
            .AsSplitQuery()
            .Where(c => c.CompanyPrefix == digitalLink.CompanyPrefix)
            .Where(p => prefixValues.Contains(p.Value) && p.Links.Any())
            .AsEnumerable();
    }

    private static LinksetResult FormatLinkset(DigitalLink digitalLink, IEnumerable<Prefix> candidates, DateTimeOffset applicability)
    {
        var links = candidates.OrderByDescending(p => p.Value.Length)
            .Aggregate(Enumerable.Empty<Link>(), (acc, p) => acc.Union(p.Links.Where(l => l.IsApplicableAt(applicability) && !acc.Any(link => link.LinkType == l.LinkType))));
        var formattedLinks = digitalLink.FormatUriTemplates(links);

        return new LinksetResult(formattedLinks);
    }

    private static IEnumerable<Link> FindMatchingLinks(IEnumerable<Prefix> candidates, DateTimeOffset applicability, params string?[] linkTypes)
    {
        foreach(var candidate in candidates.OrderByDescending(c => c.Value.Length))
        {
            foreach(var linkType in linkTypes)
            {
                var links = candidate.Links.Where(l => l.IsApplicableAt(applicability) && l.LinkType == linkType);

                if (links.Any())
                {
                    return links;
                }
            }
        }

        return [];
    }

    private static IEnumerable<Link> FilterByLanguage(IEnumerable<Link> matchingLinks, IEnumerable<LanguagePreference> languages)
    {
        foreach (var language in languages.OrderByDescending(l => l.Quality))
        {
            var matchingLanguagesLink = matchingLinks.Where(link => LanguageMatches(link, language));

            if (matchingLanguagesLink.Any())
            {
                return matchingLanguagesLink.Any(link => RegionMatches(link, language))
                    ? matchingLanguagesLink.Where(link => RegionMatches(link, language))
                    : matchingLanguagesLink;
            }
        }

        return matchingLinks;
    }

    private static bool RegionMatches(Link link, LanguagePreference language)
    {
        if (link.Language is null) return false;

        return language.Region is not null && link.Language.Region is not null && link.Language.Region.Equals(language.Region, StringComparison.OrdinalIgnoreCase);
    }

    private static bool LanguageMatches(Link link, LanguagePreference language)
    {
        if (link.Language is null) return false;

        return link.Language.Key.Equals(language.Language, StringComparison.OrdinalIgnoreCase);
    }

    private static readonly string[] DefaultLinkTypes = ["gs1:defaultLinkMulti", "gs1:defaultLink"];
}

public interface IResolutionResult
{
    public IEnumerable<Link> Links { get; }
}

public sealed record LinksetResult(IEnumerable<Link> Links) : IResolutionResult { };
public sealed record LinkTypeResult(IEnumerable<Link> Links) : IResolutionResult { };