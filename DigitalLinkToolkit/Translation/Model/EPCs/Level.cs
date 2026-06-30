using System.Text.Json.Serialization;

namespace DigitalLinkToolkit.Translation.Model.EPCs;

public class Level
{
    [JsonPropertyName("type")]
    public required LevelType Type { get; set; }
    [JsonPropertyName("prefixMatch")]
    public required string PrefixMatch { get; set; }
    [JsonPropertyName("requiredFormattingParameters")]
    public string? RequiredFormattingParameters { get; set; }
    [JsonPropertyName("requiredParsingParameters")]
    public string? RequiredParsingParameters { get; set; }
    [JsonPropertyName("gs1DigitalLinkKeyQualifiers")]
    public IEnumerable<string> DigitalLinkToolkitKeyQualifiers { get; set; } = [];
    [JsonPropertyName("option")]
    public IEnumerable<Option> Options { get; set; } = [];
    [JsonPropertyName("rule")]
    public IEnumerable<Rule> Rules { get; set; } = [];
}
