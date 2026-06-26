using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Goto.Infrastructure.Routing.Constraints;

public abstract class GS1ResolverRouteAttribute(bool isLinksetExpected) : HttpMethodAttribute(["GET", "HEAD"], "{**_}"), IActionConstraint
{
    public const string LinksetMediaType = "application/linkset+json";

    public bool Accept(ActionConstraintContext context)
    {
        var request = context.RouteContext.HttpContext.Request;

        if (request.Query["linkType"].Any(value => value is "linkset" or "all"))
            return isLinksetExpected;
        if (request.GetTypedHeaders().Accept.Any(header => !header.MatchesAllTypes && header.MediaType == LinksetMediaType))
            return isLinksetExpected;

        return !isLinksetExpected;
    }
}