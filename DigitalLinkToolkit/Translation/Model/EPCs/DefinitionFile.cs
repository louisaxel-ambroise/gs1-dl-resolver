using System.Text.Json.Serialization;

namespace DigitalLinkToolkit.Translation.Model.EPCs;

public class DefinitionFile
{
    [JsonPropertyName("tdt:epcTagDataTranslation")]
    public EpcTagDataTranslation? TagDataTranslation { get; set; }
}
