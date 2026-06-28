using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Goto.Infrastructure.Routing.Constraints;

[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public abstract class ResolverRouteAttribute(ResolverType resolverType) : GS1ResolverRouteAttribute(resolverType), IActionFilter
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

public sealed class LinkTypeResolverRouteAttribute() : ResolverRouteAttribute(ResolverType.LinkType);
public sealed class DefaultLinkResolverRouteAttribute() : ResolverRouteAttribute(ResolverType.DefaultLink);