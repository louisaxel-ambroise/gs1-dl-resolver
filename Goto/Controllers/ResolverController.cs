using Goto.Controllers.Results;
using Goto.Data;
using Goto.Data.Entities;
using Goto.Infrastructure;
using Goto.Infrastructure.Results;
using Goto.Infrastructure.Routing.Binding;
using Goto.Infrastructure.Routing.Constraints;
using Goto.Infrastructure.Routing.Filters;
using Goto.Services.Conversion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Goto.Controllers;

[Controller]
[TimeTraveler]
[AllowAnonymous]
[EnableCors]
public sealed class ResolverController
{
    [HttpGet(".well-known/gs1resolver")]
    [Produces(MediaTypes.Json, MediaTypes.Html)]
    public IActionResult GetMetadataInformation([FromUri] Uri metadataUrl)
    {
        return new OkObjectResult(new MetadataResult
        {
            ResolverRoot = string.Concat(metadataUrl.Scheme, "://", metadataUrl.Host),
            Name = "GOTO",
            SupportedPrimaryKeys = [ "all" ],
            LinkTypeDefaultCanBeLinkset = true,
            JsonLdContextLocation = "https://ref.gs1.org/standards/resolver/linkset-context",
            Contact = new()
            {
                Fn = "GOTO"
            }
        });
    }

    [LinksetResolverRoute]
    [Produces(MediaTypes.Linkset, MediaTypes.Html)]
    public IActionResult ResolveLinkset(
        [FromUri] DigitalLink digitalLink, 
        [FromServices] Context context)
    {
        var anchors = context.QueryForLink(digitalLink).ToList();
        var linksets = anchors.Select(a => new LinksetResultAnchor()
        {
            Anchor = string.Join('/', digitalLink.HostUrl, a.Prefix),
            Links = LinksetLink.Map(a.Links, digitalLink)
        }).ToList();

        foreach (var linkset in linksets.Where(a => !a.Links.Any(l => l.IsDefault)))
        {
            linkset.Links.Add(new LinksetLink
            {
                Href = digitalLink.BuildLinksetLink(),
                LinkType = "gs1:defaultLink",
                Title = "Linkset",
                IsDefault = true
            });
        }

        return linksets.Count > 0
            ? new OkObjectResult(new LinksetResult { Anchors = linksets.OrderByDescending(a => a.Anchor.Length).ToList() })
            : new NotFoundObjectResult(ErrorResponse.NotFound);
    }

    [InsightsTracking]
    [LinkTypeResolverRoute]
    [Produces(MediaTypes.Json, MediaTypes.Html)]
    public IActionResult ResolveDigitalLinkAsync(
        [FromUri] DigitalLink digitalLink, 
        [FromQuery] string linkType, 
        [FromHeader] string[] mediaTypes, 
        [FromHeader] Language[] languages, 
        [FromServices] Context context)
    {
        var anchors = context.QueryForLink(digitalLink)
            .Where(a => a.Links.Any(l => l.LinkType == linkType || l.IsDefault))
            .ToList();

        foreach (var anchor in anchors.OrderByDescending(a => a.Prefix.Length))
        {
            var links = ResolutionResultLink.Map(anchor.FindBestMatches(linkType, languages, mediaTypes), digitalLink);
            
            if (links.Count == 1)
                return new RedirectResult(links.Single().Href);
            if (links.Count > 1) 
                return new MultipleChoicesObjectResult(new ResolutionResult { Links = links });
            if (string.IsNullOrEmpty(linkType))
                return new RedirectResult(digitalLink.BuildLinksetLink());
        }

        return new NotFoundObjectResult(ErrorResponse.NotFound);
    }
}
