namespace Gs1DigitalLink.Core.Model;

public sealed class Insight : Aggregate
{
    public required string DigitalLink { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public string? LinkType { get; init; }
    public IEnumerable<string> Languages { get; init; } = [];
    public int CandidateCount { get; init; }
}
