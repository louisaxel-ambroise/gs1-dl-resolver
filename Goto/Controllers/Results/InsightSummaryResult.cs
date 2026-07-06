namespace Goto.Controllers.Results;

public sealed class InsightSummaryResult
{
    public required int Count { get; set; }
    public required List<InsightSummary> Data { get; set; } = [];
}

public sealed class InsightSummary
{
    public required string DigitalLink { get; set; }
    public required int ScanCount { get; set; }
}
