using System.Text.Json.Serialization;

namespace FasTnT.TagDataTranslation.Model.EPCs;

public class Option
{
    [JsonPropertyName("optionKey")]
    public required string OptionKey { get; set; }
    [JsonPropertyName("pattern")]
    public required string Pattern { get; set; }
    [JsonPropertyName("grammar")]
    public required string Grammar { get; set; }
    [JsonPropertyName("aiSequence")]
    public IEnumerable<string> AISequence { get; set; } = [];
    [JsonPropertyName("encodedAI")]
    public IEnumerable<EncodedAI> EncodedAIs { get; set; } = [];
    [JsonPropertyName("field")]
    public IEnumerable<Field> Fields { get; set; } = [];
}
