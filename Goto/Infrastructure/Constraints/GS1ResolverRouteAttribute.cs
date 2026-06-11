using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.Net.Http.Headers;

namespace Goto.Infrastructure.Constraints;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class GS1ResolverRouteAttribute() : HttpGetAttribute("{**_:minlength(2)}"), IActionConstraint
{
    public bool IsLinksetRequired { get; init; }

    public bool Accept(ActionConstraintContext context)
    {
        var request = context.RouteContext.HttpContext.Request;

        if (request.Headers[HeaderNames.Accept] == "application/linkset+json")
            return IsLinksetRequired;
        if (request.Query["linkType"].FirstOrDefault() is "linkset" or "all")
            return IsLinksetRequired;

        return !IsLinksetRequired;
    }
}