using Microsoft.AspNetCore.Mvc.Filters;

namespace Goto.Infrastructure.Filters;

public sealed class ValidateRequestAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var message = string.Join(" | ", context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            throw new BadHttpRequestException(message, 400);
        }
    }
}