using Gs1DigitalLink.Core.Contracts.Conversion;
using Gs1DigitalLink.Core.Contracts.Resolution;
using Gs1DigitalLink.Core.Model;
using Gs1DigitalLink.Core.Services.Conversion;
using Gs1DigitalLink.Core.Services.Resolution;
using Gs1DigitalLink.Web.Contracts;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Gs1DigitalLink.Web.Controllers;

[ApiController]
[Produces("application/json", "application/linkset+json", "text/html")]
public sealed class ResolverController(IDigitalLinkConverter converter, IDigitalLinkResolver resolver) : ControllerBase
{
    private const string LinkTypeQueryKey = "linkType";

    [HttpGet, HttpHead]
    [Route("{**_:minlength(2)}")]
    public IActionResult HandleRequest()
    {
        var digitalLink = converter.Parse(Request.GetDisplayUrl());
        var applicability = Request.GetApplicableDate();
        var queryElements = Request.Query.Where(s => !Equals(LinkTypeQueryKey, s.Key)).ToDictionary(kv => kv.Key, kv => (string?) kv.Value.ToString());

        var result = Request.IsLinksetRequested()
            ? resolver.ResolveLinkSet(digitalLink, applicability)
            : resolver.ResolveLinkType(digitalLink, applicability, Request.Query[LinkTypeQueryKey]);

        var formattedLinks = result.Links.Select(l => $"<{QueryHelpers.AddQueryString(l.RedirectUrl, queryElements)}>; rel=\"{l.LinkType}\";{(l.Language is null ? "" : "hreflang=\"" + l.Language + "\"")}").ToList();
        
        AppendLinkHeaders(digitalLink, result);

        return HttpMethods.IsHead(Request.Method)
            ? Ok()
            : Format(digitalLink, result, queryElements);
    }

    #region Util Methods

    private void AppendLinkHeaders(DigitalLink digitalLink, IResolutionResult result)
    {
        if (result is LinksetResult)
        {
            Response.Headers.Append("Link", "<https://ref.gs1.org/standards/resolver/linkset-context>; rel=\"http://www.w3.org/ns/json-ld#context\"; type=\"application/ld+json\"");
        }
        else
        {
            Response.Headers.Append("Link", $"<{Request.Scheme}://{Request.Host}/{digitalLink.ToShortString()}>; rel=\"linkset\"; type=\"application/linkset+json\"");
        }
    }

    private IActionResult Format(DigitalLink digitalLink, IResolutionResult result, IDictionary<string, string?> queryElements)
    {
        return result switch
        {
            LinksetResult => new OkObjectResult(MapLinksetResponse(digitalLink, result, queryElements)),
            LinkTypeResult r when r.Links.Count() > 1 => new ObjectResult(new MultipleChoiceResponse(MapLink(result.Links, queryElements))) { StatusCode = StatusCodes.Status300MultipleChoices },
            LinkTypeResult r when r.Links.Count() == 1 => new RedirectResult(QueryHelpers.AddQueryString(result.Links.Single().RedirectUrl, queryElements), false, true),
            LinkTypeResult r when !r.Links.Any() => new NotFoundObjectResult(ErrorResponse.NotFound),
            _ => new ObjectResult(ErrorResponse.InternalServerError)
        };
    }

    private LinksetResponse MapLinksetResponse(DigitalLink digitalLink, IResolutionResult result, IDictionary<string, string?> queryElements)
    {
        var anchor = $"{Request.Scheme}://{Request.Host}/{digitalLink.ToShortString()}";
        var links = result.Links.GroupBy(l => l.LinkType).ToDictionary(g => g.Key, g => MapLink(g, queryElements));
        
        return new LinksetResponse(anchor, links);
    }

    private static IEnumerable<LinkDefinition> MapLink(IEnumerable<Link> links, IDictionary<string, string?> queryElements)
    {
        return links.GroupBy(l => new { l.LinkType, l.RedirectUrl, l.Title })
            .Select(g => new LinkDefinition
            {
                Hreflang = [.. g.Where(l => l.Language is not null).Select(l => l.Language!.ToString())],
                Href = QueryHelpers.AddQueryString(g.Key.RedirectUrl, queryElements),
                Title = g.Key.Title
            });
    }

    #endregion
}
