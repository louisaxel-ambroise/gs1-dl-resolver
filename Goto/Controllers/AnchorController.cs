using DigitalLinkToolkit.Conversion;
using Goto.Controllers.Requests;
using Goto.Services.Data.Entities;
using Goto.Infrastructure;
using Goto.Infrastructure.Routing.Filters;
using Goto.Services;
using Goto.Services.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sqids;
using System.Security.Claims;

namespace Goto.Controllers;

[TransactionalController]
[ValidateRequest]
[Route("api/anchors")]
[Produces(MediaTypes.Json)]
[Authorize(AuthenticationSchemes = "ApiKey")]
public sealed class AnchorController(Context context, Clock clock, ClaimsPrincipal principal, SqidsEncoder<int> encoder)
{
    [HttpGet]
    public IActionResult ListAnchors()
    {
        var anchors = context.AnchorsForUser(principal)
            .OrderBy(a => a.Id)
            .Select(a => new
            {
                Id = encoder.Encode(a.Id),
                a.Prefix,
                a.Description
            });

        return new OkObjectResult(anchors);
    }


    [HttpPost]
    public IActionResult CreateAnchor([FromBody] AddAnchorRequest request, [FromServices] IdentifierConverter converter)
    {
        var identifier = converter.Parse(request.Prefix);

        var anchor = new Anchor
        {
            Prefix = identifier.Value,
            CompanyPrefix = principal.GetCompanyPrefix(),
            Description = request.Description
        };
        context.Add(anchor);

        return new CreatedResult();
    }

    [HttpDelete("{anchorKey}")]
    public IActionResult CleanAnchor([FromRoute] string anchorKey)
    {
        var anchorId = encoder.Decode(anchorKey).Single();
        var anchors = context.AnchorsForUser(principal).First(a => a.Id == anchorId);

        foreach(var activeLink in anchors.Links.Where(l => l.ActiveUntil >= clock.UtcNow))
        {
            activeLink.ActiveUntil = DateTimeOffset.Min(activeLink.ActiveFrom, clock.UtcNow);
        }

        return new NoContentResult();
    }

    [HttpGet("{anchorKey}")]
    public IActionResult GetAnchorDetails([FromRoute] string anchorKey)
    {
        var anchorId = encoder.Decode(anchorKey).Single();
        var anchor = context.AnchorsForUser(principal).First(a => a.Id == anchorId);

        return new OkObjectResult(new
        {
            Id = encoder.Encode(anchor.Id),
            anchor.Prefix,
            anchor.Description,
            Links = anchor.Links.OrderBy(l => l.Id)
                .Select(l => new
                {
                    Id = encoder.Encode(anchorId, l.Id),
                    l.ActiveFrom,
                    l.ActiveUntil,
                    l.RedirectUrl,
                    l.Title,
                    l.LinkType,
                    Language = l.Language.ToString(),
                    l.MediaType,
                    l.IsDefault
                })
        });
    }

    [HttpPost("{anchorKey}/links")]
    public IActionResult AddAnchorLink([FromRoute] string anchorKey, [FromBody] AddAnchorLinkRequest request)
    {
        var anchorId = encoder.Decode(anchorKey).Single();
        var anchor = context.AnchorsForUser(principal).First(a => a.Id == anchorId);

        anchor.Links.AddRange(request.ToAnchorLinks());

        return new CreatedResult();
    }

    [HttpDelete("{anchorKey}/links/{linkKey}")]
    public IActionResult RemoveAnchorLink([FromRoute] string anchorKey, [FromRoute] string linkKey)
    {
        var anchorId = encoder.Decode(anchorKey).Single();
        var linkIds = encoder.Decode(linkKey);

        if (linkIds[0] != anchorId)
            throw new InvalidOperationException("Invalid IDs");

        var anchor = context.AnchorsForUser(principal).AsTracking().First(a => a.Id == anchorId );
        var link = anchor.Links.Single(l => l.Id == linkIds[^1]);

        link.SetUnavailabilityDate(clock.UtcNow);

        return new NoContentResult();
    }
}
