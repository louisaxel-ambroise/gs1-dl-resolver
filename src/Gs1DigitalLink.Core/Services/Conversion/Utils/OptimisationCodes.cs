using System.Text.Json.Serialization;

namespace Gs1DigitalLink.Core.Services.Conversion.Utils;

internal sealed class OptimizationCodes
{
    [JsonPropertyName("OptimizationCodes")]
    public required IReadOnlyList<OptimizationCode> Codes { get; init; }

    public bool TryGet(string code, out OptimizationCode result)
    {
        result = Codes.SingleOrDefault(x => x.Code == code, OptimizationCode.Default);

        return result != OptimizationCode.Default;
    }

    public bool TryGetMatching(IEnumerable<string> ais, out OptimizationCode result)
    {
        result = Codes
            .OrderByDescending(x => x.Priority)
            .FirstOrDefault(x => x.IsFulfilledBy(ais), OptimizationCode.Default);

        return result != OptimizationCode.Default;
    }

    internal record OptimizationCode
    {
        public required string Code { get; init; }
        public required string[] SequenceAIs { get; init; }
        public required string Meaning { get; init; }
        public required string Usage { get; init; }

        public int Priority => SequenceAIs.Length;

        public bool IsFulfilledBy(IEnumerable<string> identifierCodes) => SequenceAIs.All(identifierCodes.Contains);

        public static readonly OptimizationCode Default = new()
        {
            Code = string.Empty,
            SequenceAIs = [],
            Meaning = string.Empty,
            Usage = string.Empty
        };
    }
}
