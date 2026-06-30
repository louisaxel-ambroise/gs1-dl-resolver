using System.Text.Json.Serialization;

namespace FasTnT.TagDataTranslation.Model.EPCs;

public class Scheme
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("optionKey")]
    public string? OptionKey { get; set; }
    [JsonPropertyName("tagLength")]
    public int? TagLength { get; set; }
    [JsonPropertyName("level")]
    public required IEnumerable<Level> Levels { get; set; }
}
