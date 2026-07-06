using Goto.Services.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Goto.Infrastructure.Routing.Filters;

public sealed class TransactionalControllerAttribute : ControllerAttribute, IActionFilter
{
    static readonly string[] WriteMethods = [ HttpMethods.Post, HttpMethods.Put, HttpMethods.Delete ];

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (WriteMethods.Contains(context.HttpContext.Request.Method))
        {
            var databaseContext = context.HttpContext.RequestServices.GetRequiredService<Context>();

            databaseContext.SaveChanges();
        }
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (WriteMethods.Contains(context.HttpContext.Request.Method))
        {
            var databaseContext = context.HttpContext.RequestServices.GetRequiredService<Context>();
            databaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        }
    }
}
