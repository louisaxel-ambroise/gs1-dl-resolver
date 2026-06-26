using Goto.Controllers.Results;
using Goto.Data.Entities;
using Goto.Infrastructure.Results;
using Goto.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Channels;

namespace Goto.Infrastructure.Routing.Filters;

public sealed class InsightsTrackingAttribute : ActionFilterAttribute
{
    const string HeaderName = "X-Request-Tracking";

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (BypassTracking(context.HttpContext.Request)) 
            return;

        var channel = context.HttpContext.RequestServices.GetRequiredService<Channel<Insight>>();
        var insight = context.Result switch
        {
            MultipleChoicesObjectResult multipleChoices => CreateInsight(context.HttpContext, 300, (multipleChoices.Value as ResolutionResult)?.Links.Count ?? 0),
            RedirectResult => CreateInsight(context.HttpContext, 307, 1),
            NotFoundObjectResult => CreateInsight(context.HttpContext, 404, 0),
            _ => CreateInsight(context.HttpContext, 500, 0)
        };

        channel.Writer.TryWrite(insight);
    }

    private static Insight CreateInsight(HttpContext context, int statusCode, int linkCount)
    {
        var timeProvider = context.RequestServices.GetRequiredService<ApiTimeProvider>();

        return new Insight
        {
            RecordDate = DateTimeOffset.UtcNow,
            RequestDate = timeProvider.Now,
            StatusCode = statusCode,
            QueryString = context.Request.QueryString.Value,
            AcceptLanguage = context.Request.Headers.AcceptLanguage,
            Accept = context.Request.Headers.Accept,
            Url = context.Request.Path,
            LinkCount = linkCount
        };
    }

    private static bool BypassTracking(HttpRequest request)
    {
        return request.Headers.TryGetValue(HeaderName, out var values) 
            && values.Any(v => v is "bypass" or "notrack");
    }
}
