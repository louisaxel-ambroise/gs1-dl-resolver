using System.Text.Json.Serialization;

namespace DigitalLinkToolkit.Translation.Model.EPCs;

public class Field
{
    [JsonPropertyName("seq")]
    public required int Seq { get; set; }
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("characterSet")]
    public required string CharacterSet { get; set; }
    [JsonPropertyName("bitLength")]
    public int? BitLength { get; set; }
    [JsonPropertyName("compaction")]
    public string? Compaction { get; set; }
    [JsonPropertyName("compression")]
    public string? Compression { get; set; }
    [JsonPropertyName("padChar")]
    public string? PadChar { get; set; }
    [JsonPropertyName("padDir")]
    public Direction? PadDir { get; set; }
    [JsonPropertyName("bitPadDir")]
    public Direction? BitPadDir { get; set; }
    [JsonPropertyName("decimalMinimum")]
    public string? DecimalMinimum { get; set; }
    [JsonPropertyName("decimalMaximum")]
    public string? DecimalMaximum { get; set; }
    [JsonPropertyName("length")]
    public int? Length { get; set; }
    [JsonPropertyName("gcpOffset")]
    public int? GcpOffset { get; set; }
    [JsonPropertyName("valueIfNull")]
    public string? ValueIfNull { get; set; }
    [JsonPropertyName("encoding")]
    public string? Encoding { get; set; }
}
