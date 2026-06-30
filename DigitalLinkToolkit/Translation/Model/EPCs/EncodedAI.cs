using System.Text.Json.Serialization;

namespace DigitalLinkToolkit.Translation.Model.EPCs;

public class EncodedAI
{
    [JsonPropertyName("seq")]
    public required int Seq { get; set; }
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("ai")]
    public required string AI { get; set; }  
}
