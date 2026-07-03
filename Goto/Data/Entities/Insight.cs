namespace Goto.Data.Entities;

public sealed class Insight
{
    public int? Id { get; init; }
    public required DateTimeOffset RecordDate { get; init; }
    public required DateTimeOffset RequestDate { get; init; }
    public required string? DigitalLink { get; init; }
    public required string? CompanyPrefix { get; init; }
    public required string Url { get; init; }
    public required string? AcceptLanguage { get; init; }
    public required string? Accept { get; init; }
    public required int LinkCount { get; init; }
    public int StatusCode { get; init; }
}