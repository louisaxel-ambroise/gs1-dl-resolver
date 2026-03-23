using Gs1DigitalLink.Core.Services.Conversion;
using Gs1DigitalLink.Web.Contracts;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Gs1DigitalLink.Web.Controllers;

[Route("error")]
[Produces("application/json", "text/html")]
public sealed class ErrorController : ControllerBase
{
    [HttpGet, HttpPost, HttpPut, HttpDelete]
    public IActionResult HandleError()
    {
        var ex = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
        var problem = ex is null
            ? new ErrorResponse
            {
                Type = "InternalError",
                Title = "Unable to process the request",
                Detail = "An unexpected error occured",
                Errors = [],
                Status = (int) HttpStatusCode.InternalServerError
            }
            : new ErrorResponse
            {
                Type = "InternalError",
                Title = "Unable to process the request",
                Detail = ex.Message,
                Errors = ExtractDetailedErrors(ex),
                Status = HttpContext.Response.StatusCode
            };
        
        return new ObjectResult(problem) { StatusCode = 400 };
    }

    private static IEnumerable<ErrorDetail> ExtractDetailedErrors(Exception ex)
    {
        if (ex is not InvalidDigitalLinkException invalidDigitalLink)
        {
            return [];
        }

        return invalidDigitalLink.Issues.Select(i => new ErrorDetail { Code = i.Code, Message = i.Message });
    }
}
