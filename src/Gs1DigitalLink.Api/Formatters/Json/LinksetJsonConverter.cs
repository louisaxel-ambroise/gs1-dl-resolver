using Gs1DigitalLink.Api.Contracts;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gs1DigitalLink.Api.Formatters.Json;

public sealed class LinksetJsonConverter : JsonConverter<LinksetResponse>
{
    public override LinksetResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, LinksetResponse value, JsonSerializerOptions options)
    {
        // Custom serialization logic
        writer.WriteStartObject();

        writer.WritePropertyName("linkset");
        writer.WriteStartArray();
        writer.WriteStartObject();
        
        writer.WritePropertyName("anchor");
        writer.WriteStringValue(value.Anchor);

        foreach(var choice in value.Links)
        {
            writer.WritePropertyName(choice.Key.Replace("gs1:", "https://ref.gs1.org/voc/"));

            JsonSerializer.Serialize(writer, choice.Value, options);
        }
        writer.WriteEndObject();

        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}