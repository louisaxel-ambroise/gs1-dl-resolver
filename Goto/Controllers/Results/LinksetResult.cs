using Goto.Data.Entities;
using Goto.Services.Conversion;

namespace Goto.Controllers.Results;

public sealed class LinksetResult
{
    public required string LinksetUrl { get; set; }
    public required IEnumerable<LinksetResultAnchor> Anchors { get; set; } = [];
}

public sealed class LinksetResultAnchor
{
    public required string Anchor { get; init; }
    public required string Description { get; init; }
    public required IEnumerable<LinksetLink> Links { get; init; } = [];
}

public sealed class LinksetLink
{
    public required string LinkType { get; init; }
    public required string Title { get; init; }
    public required string Href { get; init; }
    public string[] Hreflang { get; init; } = [];
    public string? Type { get; init; }
    public bool IsDefault { get; init; }

    public static IEnumerable<LinksetLink> Map(IEnumerable<AnchorLink> links, DigitalLink digitalLink)
    {
        var mappedLinks = links.OrderBy(l => l.Id)
            .GroupBy(link => new { link.RedirectUrl, link.LinkType, link.Title, link.MediaType, link.IsDefault })
            .Select(grp => new LinksetLink
            {
                Href = digitalLink.FormatUriTemplates(grp.Key.RedirectUrl),
                LinkType = grp.Key.LinkType,
                Title = grp.Key.Title,
                Type = grp.Key.MediaType,
                IsDefault = grp.Key.IsDefault,
                Hreflang = [.. grp.Select(l => l.Language.ToString()).Distinct()]
            });
        return [.. mappedLinks];
    }
}
