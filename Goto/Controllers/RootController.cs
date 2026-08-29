using Goto.Controllers.Results;
using Goto.Services.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Goto.Controllers;

[Controller]
[AllowAnonymous]
public sealed class RootController
{
    [HttpGet("")]
    [Produces(MediaType.Html)]
    public IActionResult Root()
    {
        return new OkObjectResult(new RootResult());
    }
}