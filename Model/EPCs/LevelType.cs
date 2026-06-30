using System.Text.Json.Serialization;

namespace FasTnT.TagDataTranslation.Model.EPCs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LevelType
{
    Binary,
    Tag_Encoding,
    Pure_Identity,
    Gs1_AI_Json,
    Gs1_Digital_Link,
    Bare_Identifier,
    TEI
}
