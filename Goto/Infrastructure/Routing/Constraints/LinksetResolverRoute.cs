using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Goto.Infrastructure.Routing.Constraints;

[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public sealed class LinksetResolverRoute() : GS1ResolverRouteAttribute(true), IActionConstraint, IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.HttpContext.Response.IsSuccessStatusCode())
        {
            context.HttpContext.Response.Headers.Append("Link", "<https://ref.gs1.org/standards/resolver/linkset-context>; rel=\"http://www.w3.org/ns/json-ld#context\"; type=\"application/ld+json\"");
        }
    }
}
