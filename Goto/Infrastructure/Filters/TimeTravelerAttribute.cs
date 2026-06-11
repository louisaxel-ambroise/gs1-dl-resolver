using Goto.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Goto.Infrastructure.Filters;

public sealed class TimeTravelerAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Request.Headers.TryGetValue("X-Request-Date", out var headerDate) && DateTimeOffset.TryParse(headerDate, out var requestDate))
        {
            if (requestDate.ToUniversalTime() > DateTimeOffset.UtcNow)
            {
                throw new InvalidOperationException("Cannot time travel in the future.");
            }
            var timeProvider = context.HttpContext.RequestServices.GetRequiredService<ApiTimeProvider>();
            timeProvider.SetRequestDate(requestDate);
        }
    }
}
