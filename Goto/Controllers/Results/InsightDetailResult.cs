namespace Goto.Controllers.Results;

public sealed class InsightDetailResult
{
    public required int Count { get; init; }
    public required List<InsightDetail> Data { get; init; } = [];
}

public sealed class InsightDetail
{
    public required string Url { get; init; }
    public required DateTimeOffset RecordDate { get; init; }
    public required DateTimeOffset RequestDate { get; init; }
    public required InsightHeaders Headers { get; init; }
    public required int StatusCode { get; init; }
    public required int LinkCount { get; init; }
}

public sealed class InsightHeaders
{
    public string? AcceptLanguage { get; init; }
    public string? Accept { get; init; }
}