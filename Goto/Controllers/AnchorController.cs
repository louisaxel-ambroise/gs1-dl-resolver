using Goto.Controllers.Requests;
using Goto.Data;
using Goto.Data.Entities;
using Goto.Infrastructure.Filters;
using Goto.Services;
using Goto.Services.Conversion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Goto.Controllers;

[Controller]
[Transactional]
[ValidateRequest]
[Route("api/anchors")]
[Produces("application/json")]
public sealed class AnchorController(Context context, ApiTimeProvider timeProvider)
{
    [HttpPost]
    public IActionResult CreateAnchor([FromBody] AddAnchorRequest request, [FromServices] IdentifierConverter converter)
    {
        var identifier = converter.Parse(request.Prefix);

        context.Anchors.Add(new Anchor
        {
            Prefix = identifier.Value,
            CompanyPrefix = request.CompanyPrefix,
            Description = request.Description
        });

        return new CreatedResult();
    }

    [HttpDelete("{anchorId:int}")]
    public IActionResult CleanAnchor([FromRoute] int anchorId)
    {
        var anchors = context.Anchors
            .AsTracking()
            .IgnoreQueryFilters(["ActiveLinks"])
            .Include(a => a.Links)
            .Single(a => a.Id == anchorId);

        foreach(var activeLink in anchors.Links.Where(l => l.ActiveUntil >= timeProvider.UtcNow))
        {
            activeLink.ActiveUntil = DateTimeOffset.Min(activeLink.ActiveFrom, timeProvider.UtcNow);
        }

        return new NoContentResult();
    }

    [HttpGet]
    public IActionResult ListAnchors()
    {
        var anchors = context.Anchors
            .AsTracking()
            .Select(anchor => new
            {
                anchor.Id,
                anchor.Prefix,
                anchor.Description
            });

        return new OkObjectResult(anchors);
    }

    [HttpGet("{anchorId:int}")]
    public IActionResult GetAnchorDetails([FromRoute] int anchorId)
    {
        var anchor = context.Anchors
            .IgnoreQueryFilters(["ActiveLinks"])
            .Include(a => a.Links)
            .Single(anchor => anchor.Id == anchorId);

        return new OkObjectResult(anchor);
    }

    [HttpPost("{anchorId:int}/links")]
    public IActionResult AddAnchorLink([FromRoute] int anchorId, [FromBody] AddAnchorLinkRequest request)
    {
        var anchor = context.Anchors
            .AsTracking()
            .Single(a => a.Id == anchorId);

        anchor.Links.AddRange(request.ToAnchorLinks());

        return new CreatedResult();
    }

    [HttpDelete("{anchorId:int}/links/{linkId:int}")]
    public IActionResult RemoveAnchorLink([FromRoute] int anchorId, [FromRoute] int linkId)
    {
        var anchor = context.Anchors
            .IgnoreQueryFilters(["ActiveLinks"])
            .AsTracking()
            .Include(a => a.Links.Where(l => l.Id == linkId))
            .Single(a => a.Id == anchorId);
        var link = anchor.Links.Single(l => l.Id == linkId);

        link.SetUnavailabilityDate(timeProvider.UtcNow);

        return new NoContentResult();
    }
}
