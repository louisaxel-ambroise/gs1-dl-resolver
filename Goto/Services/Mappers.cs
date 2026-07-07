using DigitalLinkToolkit.Conversion.Model;
using Goto.Controllers.Requests;
using Goto.Controllers.Results;
using Goto.Services.Data.Entities;

namespace Goto.Services;

public static class Mappers
{
    public static IEnumerable<LinksetLink> Map(this IEnumerable<AnchorLink> links, DigitalLink digitalLink)
    {
        return links.Select(l => Map(l, digitalLink));
    }

    public static LinksetLink Map(AnchorLink link, DigitalLink digitalLink)
    {
        return new()
        {
            Href = digitalLink.FormatUriTemplates(link.RedirectUrl),
            LinkType = link.LinkType.ToString(),
            Title = link.Title,
            Type = link.MediaType.ToString(),
            IsDefault = link.IsDefault,
            Hreflang = [link.Language.ToString()]
        };
    }

    public static IEnumerable<AnchorLink> ToAnchorLinks(this AddAnchorLinkRequest request)
    {
        return request.Languages.Select(language => new AnchorLink
        {
            LinkType = LinkType.Parse(request.Type),
            RedirectUrl = request.RedirectUrl,
            Title = request.Title,
            Language = Language.Parse(language),
            MediaType = MediaType.Parse(request.MediaType),
            ActiveFrom = DateTimeOffset.Min(request.ActiveFrom?.ToUniversalTime(), DateTimeOffset.UtcNow),
            ActiveUntil = DateTimeOffset.Min(request.ActiveUntil?.ToUniversalTime(), DateTimeOffset.MaxValue),
            IsDefault = request.IsDefault
        });
    }
}
