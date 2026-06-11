using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Goto.Infrastructure.Filters;

public sealed class ResponseLinksetHeaderAttribute : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.HttpContext.Response.StatusCode is >= 200 and < 400)
        {
            context.HttpContext.Response.Headers.Append("Link", $"<{context.HttpContext.Request.GetDisplayUrl()}>; rel=\"linkset\"; type=\"application/linkset+json\"");
        }
    }
}
