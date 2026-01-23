using Gs1DigitalLink.Api.Contracts;
using Gs1DigitalLink.Core.Model;
using Gs1DigitalLink.Core.Services.Conversion;
using Gs1DigitalLink.Core.Services.Registration;
using Microsoft.AspNetCore.Mvc;

namespace Gs1DigitalLink.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RegisterController(IDigitalLinkPrefixConverter converter, ILinkRegistrator registrator, TimeProvider timeProvider) : ControllerBase
{
    [HttpPost]
    public IActionResult Register([FromBody] RegisterLinkDefinitionRequest request)
    {
        var digitalLink = converter.Parse(request.Prefix);
        var applicability = MapApplicability(request.Applicability);

        registrator.RegisterLink(digitalLink, request.RedirectUrl, request.Title, request.Language, applicability, request.LinkTypes);

        return new NoContentResult();
    }

    [HttpDelete]
    public IActionResult Delete([FromBody] RemoveLinkDefinitionRequest request)
    {
        var digitalLink = converter.Parse(request.Prefix);

        registrator.DeleteLink(digitalLink, request.Language, request.LinkTypes);

        return new NoContentResult();
    }

    private DateRange MapApplicability(RegisterLinkApplicability? applicability)
    {
        return new (applicability?.From ?? timeProvider.GetUtcNow(), applicability?.To);
    }
}
