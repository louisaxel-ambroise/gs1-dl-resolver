using Goto.Controllers.Results;
using Goto.Infrastructure.Routing.Binding;
using Goto.Services.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Goto.Controllers;

[Controller]
[EnableCors]
[AllowAnonymous]
public sealed class MetadataController
{
    [HttpGet("/")]
    public IActionResult Root()
    {
        return new ContentResult() { Content = File.ReadAllText("wwwroot/index.html"), ContentType = "text/html", StatusCode = 200 };
    }

    [HttpGet(".well-known/gs1resolver")]
    [Produces(MediaType.Json)]
    public IActionResult GetMetadataInformation([FromUri] Uri metadataUrl)
    {
        return new OkObjectResult(new MetadataResult
        {
            ResolverRoot = string.Concat(metadataUrl.Scheme, "://", metadataUrl.Host),
            Name = "GOTO",
            SupportedPrimaryKeys = ["all"],
            LinkTypeDefaultCanBeLinkset = true,
            JsonLdContextLocation = "https://ref.gs1.org/standards/resolver/linkset-context",
            Contact = new()
            {
                Fn = "GOTO"
            }
        });
    }

}