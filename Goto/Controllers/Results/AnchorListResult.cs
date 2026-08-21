namespace Goto.Controllers.Results;

public sealed class AnchorListResult
{
    public List<AnchorResult> Anchors { get; set; } = [];
}

public sealed class AnchorResult
{
    public required string Id { get; set; }
    public required string CompanyPrefix { get; set; }
    public required string Prefix { get; set; }
    public required string Description { get; set; }
}

public sealed class AnchorDetailResult
{
    public required string Id { get; set; }
    public required string Prefix { get; set; }
    public required string Description { get; set; }
    public required IEnumerable<AnchorLinkResult> Links { get; set; }
}

public sealed class AnchorLinkResult
{
    public required string Id { get; set; }
    public required DateTimeOffset ActiveFrom { get; set; }
    public required DateTimeOffset ActiveUntil { get; set; }
    public required string RedirectUrl { get; set; }
    public required string Title { get; set; }
    public required string LinkType { get; set; }
    public required string Language { get; set; }
    public required string MediaType { get; set; }
    public required bool IsDefault { get; set; }
}