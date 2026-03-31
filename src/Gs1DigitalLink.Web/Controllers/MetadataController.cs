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
        var result = new
        {
            resolverRoot = $"{Request.Scheme}://{Request.Host}",
            name = options.Value.Name,
            supportedPrimaryKeys = options.Value.SupportedPrimaryKeys,
            linkTypeDefaultCanBeLinkset = true,
            contact = new
            {
                fn = options.Value.ContactName
            }
        };

        return new OkObjectResult(result);
    }
}
