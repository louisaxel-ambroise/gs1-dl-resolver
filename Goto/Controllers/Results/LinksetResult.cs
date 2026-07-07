namespace Goto.Controllers.Results;

public sealed class LinksetResult
{
    public required string LinksetUrl { get; set; }
    public required IEnumerable<LinksetResultAnchor> Anchors { get; set; } = [];
}

public sealed class LinksetResultAnchor
{
    public required string Anchor { get; init; }
    public required string Description { get; init; }
    public required IEnumerable<LinksetLink> Links { get; init; } = [];
}

public sealed class LinksetLink
{
    public required string LinkType { get; init; }
    public required string Title { get; init; }
    public required string Href { get; init; }
    public string[] Hreflang { get; init; } = [];
    public string? Type { get; init; }
    public bool IsDefault { get; init; }
}
