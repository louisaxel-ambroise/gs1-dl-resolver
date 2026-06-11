using Microsoft.AspNetCore.Mvc.Filters;

namespace Goto.Infrastructure.Filters;

public sealed class ResponseLinksetSchemaHeaderAttribute : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.HttpContext.Response.StatusCode is >= 200 and < 400)
        {
            context.HttpContext.Response.Headers.Append("Link", "<https://ref.gs1.org/standards/resolver/linkset-context>; rel=\"http://www.w3.org/ns/json-ld#context\"; type=\"application/ld+json\"");
        }
    }
}