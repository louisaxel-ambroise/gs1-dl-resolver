using System.Text.Json.Serialization;

namespace FasTnT.TagDataTranslation.Model.EPCs;

public class DefinitionFile
{
    [JsonPropertyName("tdt:epcTagDataTranslation")]
    public EpcTagDataTranslation TagDataTranslation { get; set; }
}
