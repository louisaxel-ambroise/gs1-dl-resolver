using Goto.Controllers.Results;
using Goto.Infrastructure;
using Goto.Infrastructure.Results;
using Goto.Infrastructure.Routing.Binding;
using Goto.Infrastructure.Routing.Constraints;
using Goto.Infrastructure.Routing.Filters;
using DigitalLinkToolkit.Conversion.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Goto.Services.Data;
using Goto.Services.Data.Entities;

namespace Goto.Controllers;

[Controller]
[TimeTraveler]
public sealed class ResolverController
{
    [LinksetResolverRoute]
    [Produces(MediaTypes.Linkset, MediaTypes.Html)]
    public IActionResult ResolveLinkset([FromUri] DigitalLink digitalLink, [FromServices] Context context)
    {
        var anchors = context.AnchorsForLink(digitalLink).ToList();
        var linksets = anchors.Select(a => new LinksetResultAnchor
        {
            Anchor = string.Join('/', digitalLink.HostUrl, a.Prefix),
            Description = a.Description,
            Links = LinksetLink.Map(a.Links, digitalLink)
        }).ToList();

        return linksets.Count > 0
            ? new OkObjectResult(new LinksetResult { LinksetUrl = digitalLink.BuildLinksetLink(), Anchors = linksets })
            : new NotFoundObjectResult(ErrorResponse.NotFound);
    }

    [InsightsTracking]
    [LinkTypeResolverRoute]
    [Produces(MediaTypes.Json, MediaTypes.Html)]
    public IActionResult ResolveLinkTypeAsync(
        [FromUri] DigitalLink digitalLink, 
        [FromQuery] string linkType, 
        [FromHeader] string[] mediaTypes, 
        [FromHeader] Language[] languages,
        [FromServices] Context context)
    {
        var anchors = context.AnchorsForLink(digitalLink)
            .Include(a => a.Links.Where(l => l.LinkType == linkType))
            .Where(a => a.Links.Any(l => l.LinkType == linkType))
            .ToList();

        foreach (var anchor in anchors)
        {
            var bestMatch = anchor.FindBestMatches(linkType, languages, mediaTypes);
            var links = ResolutionResultLink.Map(bestMatch, digitalLink);
            
            if (links.Count() == 1)
                return new RedirectResult(links.Single().Href, permanent: false, preserveMethod: true);
            if (links.Count() > 1)
                return new MultipleChoicesObjectResult(new ResolutionResult
                {
                    Description = anchor.Description,
                    Anchor = string.Join('/', digitalLink.HostUrl, anchor.Prefix),
                    Links = links
                });
        }

        return new NotFoundObjectResult(ErrorResponse.NotFound);
    }

    [InsightsTracking]
    [DefaultLinkResolverRoute]
    [Produces(MediaTypes.Json, MediaTypes.Html)]
    public IActionResult ResolveDefaultLinkAsync(
        [FromUri] DigitalLink digitalLink, 
        [FromHeader] string[] mediaTypes, 
        [FromHeader] Language[] languages,
        [FromServices] Context context)
    {
        var anchor = context.AnchorsForLink(digitalLink)
            .Include(a => a.Links.Where(l => l.IsDefault))
            .FirstOrDefault();

        if (anchor is null)
            return new NotFoundObjectResult(ErrorResponse.NotFound);
        
        var bestMatch = anchor.FindBestMatches(languages, mediaTypes);
        var links = ResolutionResultLink.Map(bestMatch.Take(1), digitalLink);
        var defaultLink = links.FirstOrDefault()?.Href ?? digitalLink.BuildLinksetLink();

        return new RedirectResult(defaultLink, permanent: false, preserveMethod: true) ;
    }
}
