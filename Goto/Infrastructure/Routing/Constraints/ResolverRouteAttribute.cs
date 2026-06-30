using DigitalLinkToolkit.Conversion;
using DigitalLinkToolkit.Conversion.Model;
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
            var digitalLink = GetDigitalLink(context.HttpContext);
            context.HttpContext.Response.Headers.Append("Link", $"<{digitalLink.BuildLinksetLink()}>; rel=\"linkset\"; type=\"application/linkset+json\"");
        }
    }

    private static DigitalLink GetDigitalLink(HttpContext httpContext)
    {
        if(!httpContext.Items.TryGetValue("gs1:digitalLink", out var item) || item is not DigitalLink digitalLink)
        {
            var converter = httpContext.RequestServices.GetRequiredService<DigitalLinkConverter>();
            digitalLink = converter.Parse(httpContext.Request);
        }

        return digitalLink;
    }
}

public sealed class LinkTypeResolverRouteAttribute() : ResolverRouteAttribute(ResolverType.LinkType);
public sealed class DefaultLinkResolverRouteAttribute() : ResolverRouteAttribute(ResolverType.DefaultLink);