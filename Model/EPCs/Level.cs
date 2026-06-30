using System.Text.Json.Serialization;

namespace FasTnT.TagDataTranslation.Model.EPCs;

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
    public IEnumerable<string> GS1DigitalLinkKeyQualifiers { get; set; } = [];
    [JsonPropertyName("option")]
    public IEnumerable<Option> Options { get; set; } = [];
    [JsonPropertyName("rule")]
    public IEnumerable<Rule> Rules { get; set; } = [];
}

public class LevelProcessor
{
    public Scheme Scheme { get; set; }
    public IEnumerable<Level> Levels { get; set; } = [];
}

public class OptionProcessor
{
    public Scheme Scheme { get; set; }
    public Level Level { get; set; }
    public Option Option { get; set; }
}