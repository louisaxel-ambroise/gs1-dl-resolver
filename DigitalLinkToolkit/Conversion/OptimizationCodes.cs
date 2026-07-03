using System.Text.Json;
using System.Text.Json.Serialization;

namespace DigitalLinkToolkit.Conversion;

public record OptimizationCodes
{
    public static OptimizationCodes Shared { get; private set; } = new();

    [JsonPropertyName("OptimizationCodes")]
    public IReadOnlyList<OptimizationCode> Codes { get; init; } = [];

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

    public record OptimizationCode
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

    public static void Initialize(string fileName, JsonSerializerOptions? options)
    {
        using var fileStream = File.OpenRead(fileName);

        Shared = JsonSerializer.Deserialize<OptimizationCodes>(fileStream, options) ?? new();
    }
}