using Goto.Controllers.Results;
using Goto.Infrastructure;
using Goto.Services.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Goto.Controllers;

[Controller]
[Route("api/insights")]
[Produces(MediaTypes.Json)]
[Authorize(AuthenticationSchemes = "ApiKey")]
public sealed class InsightsController
{
    [HttpGet]
    public IActionResult ListInsights([FromServices] Context context, [FromServices] ClaimsPrincipal principal)
    {
        var insights = context.InsightsForUser(principal)
            .Where(i => !string.IsNullOrEmpty(i.DigitalLink))
            .GroupBy(i => i.DigitalLink)
            .Select(grp => new InsightSummary
            {
                DigitalLink = grp.Key ?? string.Empty,
                ScanCount = grp.Count()
            })
            .ToList();

        return new OkObjectResult(new InsightSummaryResult { Count = insights.Count, Data = insights });
    }

    [HttpGet("{**url:minlength(2)}")]
    public IActionResult GetInsightDetails([FromRoute] string url, [FromServices] Context context, [FromServices] ClaimsPrincipal principal)
    {
        var details = context.InsightsForUser(principal)
            .Where(i => i.Url == string.Concat('/', url))
            .OrderByDescending(i => i.RecordDate)
            .Select(i => new InsightDetail
            {
                Url = i.Url,
                RecordDate = i.RecordDate,
                RequestDate = i.RequestDate,
                Headers = new InsightHeaders
                {
                    AcceptLanguage = i.AcceptLanguage,
                    Accept = i.Accept
                },
                StatusCode = i.StatusCode,
                LinkCount = i.LinkCount
            })
            .ToList();

        return new OkObjectResult(new InsightDetailResult { Count = details.Count, Data = details });
    }
}
