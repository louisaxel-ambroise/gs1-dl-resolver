using Gs1DigitalLink.Api.Contracts;
using Gs1DigitalLink.Core.Model;
using Gs1DigitalLink.Core.Services.Conversion;
using Gs1DigitalLink.Core.Services.Resolution;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Gs1DigitalLink.Api.Controllers;

[ApiController]
[Produces("application/json", "application/linkset+json", "text/html")]
public sealed class ResolverController(IDigitalLinkConverter converter, IDigitalLinkResolver resolver, TimeProvider timeProvider) : ControllerBase
{
    [HttpGet, HttpHead]
    [Route("{**_:minlength(2)}")]
    public IActionResult HandleRequest()
    {
        var digitalLink = converter.Parse(Request.GetDisplayUrl());
        var applicability = Request.GetTypedHeaders().Date ?? timeProvider.GetUtcNow();

        var result = IsLinksetRequested
            ? resolver.ResolveLinkSet(digitalLink, applicability)
            : resolver.ResolveLinkType(digitalLink, applicability, Request.Query["linkType"]);

        var queryElement = Request.Query.Where(s => s.Key != "linkType"); 
        var formattedLinks = result.Links.Select(l => $"<{QueryHelpers.AddQueryString(l.RedirectUrl, queryElement)}>; rel=\"{l.LinkType}\";{(l.Language is null ? "" : "hreflang=\"" + l.Language + "\"")}").ToList();
        
        if (digitalLink.Type is not DigitalLinkType.Uncompressed)
        {
            var uncompressedUrl = QueryHelpers.AddQueryString($"{Request.Scheme}://{Request.Host}/{digitalLink.ToString(false)}", HttpContext.Request.Query.Where(s => s.Key != "linkType"));
            Response.Headers.Append("Link", $"<{uncompressedUrl}>; rel=\"owl:sameAs\"");
        }
        Response.Headers.AppendList("Link", formattedLinks);

        if((result is Core.Services.Resolution.LinksetResult))
        {
            Response.Headers.Append("Link", "<https://ref.gs1.org/standards/resolver/linkset-context>; rel=\"http://www.w3.org/ns/json-ld#context\"; type=\"application/ld+json\"");
        }

        return Request.Method == HttpMethod.Head.Method
            ? Ok()
            : Format(digitalLink, result);
    }

    private IActionResult Format(DigitalLink digitalLink, IResolutionResult result)
    {
        var queryElement = Request.Query.Where(s => s.Key != "linkType");

        return result switch
        {
            LinksetResult => new OkObjectResult(new LinksetResponse($"{Request.Scheme}://{Request.Host}/{digitalLink.ToString(false)}", result.Links.GroupBy(l => l.LinkType).ToDictionary(g => g.Key, g => g.Select(MapLink)))),
            LinkTypeResult r when r.Links.Count() > 1 => new ObjectResult(new MultipleChoiceResponse(result.Links.Select(MapLink))) { StatusCode = StatusCodes.Status300MultipleChoices },
            LinkTypeResult r when r.Links.Count() == 1 => new RedirectResult(QueryHelpers.AddQueryString(result.Links.Single().RedirectUrl, queryElement), false, true),
            LinkTypeResult r when r.Links.Count() == 0 => new NotFoundObjectResult(ErrorResponse.NotFound),
            _ => new ObjectResult(ErrorResponse.InternalServerError)
        };
    }

    private bool IsLinksetRequested => Request.GetTypedHeaders().Accept.Any(a => a.MediaType == "application/linkset+json") 
                                   || Request.Query["linkType"].ToString() is "linkset" or "all";

    private LinkDefinition MapLink(Link link)
    {
        return new LinkDefinition
        {
            Hreflang = link.Language is null ? [] : [ link.Language.ToString() ],
            Href = QueryHelpers.AddQueryString(link.RedirectUrl, HttpContext.Request.Query.Where(s => s.Key != "linkType")),
            Title = link.Title
        };
    }
}
