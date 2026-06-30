using Goto.Data.Entities;

namespace Goto.Controllers.Requests;

public sealed class AddAnchorLinkRequest
{
    public required string RedirectUrl { get; init; }
    public required string Title { get; init; }
    public required string LinkType { get; init; }
    public string[] Languages { get; init; } = [];
    public string? MediaType { get; init; }
    public DateTimeOffset? ActiveFrom { get; init; }
    public DateTimeOffset? ActiveUntil { get; init; }
    public bool IsDefault { get; init; }

    internal IEnumerable<AnchorLink> ToAnchorLinks()
    {
        return Languages.Select(language => new AnchorLink
        {
            LinkType = LinkType,
            RedirectUrl = RedirectUrl,
            Title = Title,
            Language = Language.Parse(language),
            MediaType = MediaType,
            ActiveFrom = DateTimeOffset.Min(ActiveFrom?.ToUniversalTime(), DateTimeOffset.UtcNow),
            ActiveUntil = DateTimeOffset.Min(ActiveUntil?.ToUniversalTime(), DateTimeOffset.MaxValue),
            IsDefault = IsDefault
        });
    }
}
