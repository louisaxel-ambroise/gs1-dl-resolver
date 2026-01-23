using Gs1DigitalLink.Api.Contracts;
using Gs1DigitalLink.Core.Services.Conversion;
using Gs1DigitalLink.Core.Services.Insights;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Gs1DigitalLink.Api.Controllers;

[ApiController]
[Route("api/insights")]
[Produces("application/json")]
public sealed class InsightsController(IDigitalLinkConverter converter, IInsightResolver insightResolver) : ControllerBase
{
    [HttpGet("{**_}")]
    public IActionResult ListInsights(ListInsightRequest request)
    {
        var digitalLink = converter.Parse(Request.GetDisplayUrl());
        var options = new ListInsightsOptions { Days = request.Days };
        var result = insightResolver.ListInsights(digitalLink.ToString(false), options);

        return new OkObjectResult(new
        {
            ScanCount = result.Count(),
            DigitalLink = digitalLink.ToString(false),
            Insights = result.Select(MapInsight)
        });
    }

    private Insight MapInsight(Core.Model.Insight insight)
    {
        return new()
        {
            Timestamp = insight.Timestamp,
            LinkType = insight.LinkType,
            Languages = insight.Languages,
            CandidateCount = insight.CandidateCount
        };
    }
}