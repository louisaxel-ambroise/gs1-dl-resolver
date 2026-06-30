using System.Text.Json.Serialization;

namespace DigitalLinkToolkit.Translation.Model.EPCs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InputFormat
{
    String,
    Binary
}