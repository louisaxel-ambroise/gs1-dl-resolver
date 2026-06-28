using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Goto.Infrastructure.Routing.Constraints;

public abstract class GS1ResolverRouteAttribute(ResolverType resolverType) : HttpMethodAttribute(["GET", "HEAD"], "{**_}"), IActionConstraint
{
    public const string LinksetMediaType = "application/linkset+json";

    public bool Accept(ActionConstraintContext context)
    {
        var request = context.RouteContext.HttpContext.Request;

        if (request.Query["linkType"].Any(value => value is "linkset" or "all"))
            return resolverType is ResolverType.Linkset;
        if (request.GetTypedHeaders().Accept.Any(header => !header.MatchesAllTypes && header.MediaType == LinksetMediaType))
            return resolverType is ResolverType.Linkset;

        return request.Query["linkType"].Any(value => !string.IsNullOrEmpty(value))
            ? resolverType is ResolverType.LinkType
            : resolverType is ResolverType.DefaultLink;
    }
}

public enum ResolverType
{
    Linkset,
    LinkType,
    DefaultLink
}