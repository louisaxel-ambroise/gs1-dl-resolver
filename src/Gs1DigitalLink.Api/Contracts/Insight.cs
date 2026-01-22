namespace Gs1DigitalLink.Api.Contracts;

public sealed record Insight
{
    public required DateTimeOffset Timestamp { get; init; }
    public required string? LinkType { get; init; }
    public required IEnumerable<string> Languages { get; init; }
    public required int CandidateCount { get; init; }
}