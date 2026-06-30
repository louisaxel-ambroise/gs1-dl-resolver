using Goto.Data.Entities;
using DigitalLinkToolkit.Conversion.Model;

namespace Goto.Controllers.Results;

public sealed class ResolutionResult
{
    public required string Description { get; set; }
    public required string Anchor { get; set; }
    public IEnumerable<ResolutionResultLink> Links { get; init; } = [];
}

public sealed class ResolutionResultLink
{
    public required string LinkType { get; init; }
    public required string Href { get; init; }
    public required string Title { get; init; }
    public string[] Hreflang { get; init; } = [];
    public string? Type { get; init; }

    public static IEnumerable<ResolutionResultLink> Map(IEnumerable<AnchorLink> links, DigitalLink digitalLink)
    {
        return links
            .GroupBy(link => new { link.RedirectUrl, link.LinkType, link.Title, link.MediaType, link.IsDefault })
            .Select(grp => new ResolutionResultLink
            {
                Href = digitalLink.FormatUriTemplates(grp.Key.RedirectUrl),
                LinkType = grp.Key.LinkType,
                Title = grp.Key.Title,
                Type = grp.Key.MediaType,
                Hreflang = [.. grp.Select(l => l.Language.ToString()).Distinct()]
            });
    }
}