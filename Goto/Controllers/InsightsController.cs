using Goto.Data;
using Goto.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Goto.Controllers;

[Controller]
[Route("api/insights")]
[Produces(MediaTypes.Json)]
[Authorize(AuthenticationSchemes = "ApiKey")]
public sealed class InsightsController(Context context, ClaimsPrincipal principal)
{
    [HttpGet]
    public IActionResult ListInsights()
    {
        var insights = context.InsightsForUser(principal)
            .GroupBy(i => i.Url)
            .Select(grp => new
            {
                Url = grp.Key,
                ScanCount = grp.Count()
            });

        return new OkObjectResult(insights);
    }

    [HttpGet("{**url:minlength(2)}")]
    public IActionResult GetInsightDetails(string url)
    {
        var details = context.InsightsForUser(principal)
            .Where(i => i.Url == string.Concat('/', url))
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
