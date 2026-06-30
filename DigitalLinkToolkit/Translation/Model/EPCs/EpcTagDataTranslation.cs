using System.Text.Json.Serialization;

namespace DigitalLinkToolkit.Translation.Model.EPCs;

public class EpcTagDataTranslation
{
    [JsonPropertyName("version")]
    public required string Version { get; set; }
    [JsonPropertyName("date")]
    public required string Date { get; set; }
    [JsonPropertyName("epcTDSVersion")]
    public required string EpcTdsVersion { get; set; }
    [JsonPropertyName("scheme")]
    public required Scheme Scheme { get; set; }
}
