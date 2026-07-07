namespace Goto.Controllers.Requests;

public sealed class AddAnchorLinkRequest
{
    public required string RedirectUrl { get; init; }
    public required string Title { get; init; }
    public required string Type { get; init; }
    public required string[] Languages { get; init; } = [];
    public required string MediaType { get; init; }
    public DateTimeOffset? ActiveFrom { get; init; }
    public DateTimeOffset? ActiveUntil { get; init; }
    public bool IsDefault { get; init; }
}
