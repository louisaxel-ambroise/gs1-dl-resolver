using DigitalLinkToolkit.Conversion.Model;
using DigitalLinkToolkit.Exceptions;
using Goto.Controllers.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;

namespace Goto.Controllers;

[Route("error")]
[Produces("application/json", "text/html")]
public sealed class ErrorController : ControllerBase
{
    public IActionResult HandleError()
    {
        var ex = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

        var problem = GetProblemDetail(ex);

        return new ObjectResult(problem) { StatusCode = problem.Status };
    }

    private static ErrorResponse GetProblemDetail(Exception? exception)
    {
        return exception switch
        {
            InvalidDigitalLinkException ex => new ErrorResponse
            {
                Type = "BadRequest",
                Title = "The request specified an invalid DigitalLink",
                Detail = ex.Message,
                Errors = ex.Issues.Select(i => new ErrorDetail { Code = i.Code, Message = FormatMessage(i) }),
                Status = (int)HttpStatusCode.BadRequest
            },
            DbUpdateException ex => new ErrorResponse
            {
                Type = "Conflict",
                Title = "There is a conflict while registering the DigitalLink",
                Detail = ex.Message,
                Status = (int)HttpStatusCode.Conflict
            },
            var ex when ex is not null => new ErrorResponse
            {
                Type = "InternalError",
                Title = "Unable to process the request",
                Detail = ex.Message,
                Status = (int)HttpStatusCode.InternalServerError
            },
            _ => new ErrorResponse
            {
                Type = "InternalError",
                Title = "Unable to process the request",
                Detail = "An unexpected error occured",
                Status = (int)HttpStatusCode.InternalServerError
            }
        };
    }

    private static string FormatMessage(ValidationIssue issue)
    {
        var messageBuilder = new StringBuilder(issue.Message);

        if (!string.IsNullOrEmpty(issue.Key) && !string.IsNullOrEmpty(issue.Value))
        {
            messageBuilder.AppendFormat(" (Key: '{0}', Value: '{1}')", issue.Key, issue.Value);
        }
        else if (!string.IsNullOrEmpty(issue.Key))
        {
            messageBuilder.AppendFormat(" (Key: '{0}')", issue.Key);
        }
        else if (!string.IsNullOrEmpty(issue.Value))
        {
            messageBuilder.AppendFormat(" (Value: '{0}')", issue.Value);
        }

        return messageBuilder.ToString();
    }
}
