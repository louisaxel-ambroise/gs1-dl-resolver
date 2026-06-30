using System.Text.Json.Serialization;

namespace FasTnT.TagDataTranslation.Model.EPCs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RuleType
{
    Extract,
    Format
}
