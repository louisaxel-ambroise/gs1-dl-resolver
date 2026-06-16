using Gs1DigitalLink.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Gs1DigitalLink.Web.Controllers;

[ApiController]
[Route("/")]
public sealed class MetadataController(IOptions<GS1ResolverOptions> options) : ControllerBase
{
    [HttpGet]
    public IActionResult Root()
    {
        return Redirect(options.Value.MainUrl);
    }

    [HttpGet(".well-known/gs1resolver")]
    public IActionResult ResolverMetadata()
    {
        var result = new GS1ResolverResult
        {
            ResolverRoot = $"{Request.Scheme}://{Request.Host}",
            Name = options.Value.Name,
            SupportedPrimaryKeys = options.Value.SupportedPrimaryKeys,
            LinkTypeDefaultCanBeLinkset = true,
            Contact = new ()
            {
                Fn = options.Value.ContactName
            }
        };

        return new OkObjectResult(result);
    }
}
