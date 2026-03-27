namespace Gs1DigitalLink.Web.Contracts;

public sealed record ErrorResponse
{
    public required string Type { get; init; }
    public required string Title { get; init; }
    public required string Detail { get; init; }
    public IEnumerable<ErrorDetail> Errors { get; init; } = [];
    public int Status { get; init; }

    public static ErrorResponse InternalServerError => new()
    {
        Status = StatusCodes.Status500InternalServerError,
        Type = "InternalServerError",
        Title = "Unable to process the request",
        Detail = "Unable to process the request"
    };

    public static ErrorResponse NotFound => new()
    {
        Status = StatusCodes.Status404NotFound,
        Type = "NotFound",
        Title = "Not found",
        Detail = "There is no entry configured for the specified request"
    };
}

public sealed record ErrorDetail
{
    public required string Code { get; init; }
    public required string Message { get; init; }
}