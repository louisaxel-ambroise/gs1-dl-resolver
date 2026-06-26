using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Goto.Infrastructure.Routing.Constraints;

[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public sealed class LinkTypeResolverRoute() : GS1ResolverRouteAttribute(false), IActionConstraint, IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.HttpContext.Response.IsSuccessStatusCode())
        {
            context.HttpContext.Response.Headers.Append("Link", $"<{context.HttpContext.Request.GetDisplayUrl()}>; rel=\"linkset\"; type=\"application/linkset+json\"");
        }
    }
}
