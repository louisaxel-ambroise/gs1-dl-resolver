using Goto.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace Goto.Infrastructure.Filters;

public sealed class TransactionalAttribute : ActionFilterAttribute
{
    static readonly string[] WriteMethods = [ HttpMethods.Post, HttpMethods.Put, HttpMethods.Delete ];

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (WriteMethods.Contains(context.HttpContext.Request.Method))
        {
            var databaseContext = context.HttpContext.RequestServices.GetRequiredService<Context>();

            databaseContext.SaveChanges();
        }
    }
}
