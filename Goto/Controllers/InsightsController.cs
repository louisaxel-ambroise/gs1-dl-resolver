using Goto.Data;
using Goto.Infrastructure.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Goto.Controllers;

[Controller]
[Route("api/insights")]
[Produces("application/json")]
public sealed class InsightsController(Context context)
{
    [HttpGet]
    public IActionResult ListInsights()
    {
        var insights = context.Insights
            .GroupBy(i => i.Url)
            .Select(grp => new
            {
                Url = grp.Key,
                ScanCount = grp.Count(),
                Details = "details?url=" + grp.Key
            });

        return new OkObjectResult(insights);
    }

    [HttpGet("details")]
    public IActionResult GetInsightDetails([FromQuery] string url)
    {
        var details = context.Insights
            .Where(i => i.Url == url)
            .OrderByDescending(i => i.RecordDate)
            .Select(i => new
            {
                RequestedUrl = string.Concat(i.Url, i.QueryString),
                i.RecordDate,
                i.RequestDate,
                Headers = new
                {
                    i.AcceptLanguage,
                    i.Accept
                },
                i.StatusCode,
                i.LinkCount
            });

        return new OkObjectResult(details);
    }
}
