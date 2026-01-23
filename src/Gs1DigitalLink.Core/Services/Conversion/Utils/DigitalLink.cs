using System.Text.Json.Serialization;

namespace Gs1DigitalLink.Core.Services.Conversion.Utils;

public record Identifier
{
    public static readonly Identifier None = new();

    public string Code { get; init; } = string.Empty;
    public string ShortCode { get; init; } = string.Empty;
    public AIType Type { get; init; }
    public IReadOnlyList<AIComponent> Components { get; init; } = [];
    public string Pattern { get; set; } = string.Empty;
}

public record AIComponent
{
    public required Charset Type { get; init; }
    public required int Length { get; init; }
    public ComponentFlag Flags { get; set; }
    public int Gcp { get; init; }
}

[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ComponentFlag
{
    [JsonStringEnumMemberName("F")]
    FixedLength = 1,
    [JsonStringEnumMemberName("C")]
    CheckDigit = 2,
    [JsonStringEnumMemberName("G")]
    GCP = 4
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Charset
{
    Unknown,
    [JsonStringEnumMemberName("N")]
    Numeric,
    [JsonStringEnumMemberName("X")]
    Alpha
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AIType
{
    Unknown,
    [JsonStringEnumMemberName("P")]
    PrimaryKey,
    [JsonStringEnumMemberName("Q")]
    Qualifier,
    [JsonStringEnumMemberName("D")]
    DataAttribute,
}
