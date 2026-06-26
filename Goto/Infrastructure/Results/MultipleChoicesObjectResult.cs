using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Goto.Infrastructure.Results;

[DefaultStatusCode(DefaultStatusCode)]
public sealed class MultipleChoicesObjectResult : ObjectResult
{
    const int DefaultStatusCode = 300;

    public MultipleChoicesObjectResult([ActionResultObjectValue] object? value)
        : base(value)
    {
        StatusCode = DefaultStatusCode;
    }
}