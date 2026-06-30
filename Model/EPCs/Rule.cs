using System.Text.Json.Serialization;

namespace FasTnT.TagDataTranslation.Model.EPCs;

public class Rule
{
    [JsonPropertyName("seq")]
    public required int Seq { get; set; }
    [JsonPropertyName("newFieldName")]
    public required string NewFieldName { get; set; }
    [JsonPropertyName("function")]
    public required string Function { get; set; }
    [JsonPropertyName("type")]
    public RuleType Type { get; set; }
    [JsonPropertyName("inputFormat")]
    public InputFormat InputFormat { get; set; }
    [JsonPropertyName("characterSet")]
    public required string CharacterSet { get; set; }
    [JsonPropertyName("length")]
    public int? Length { get; set; }
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
}
