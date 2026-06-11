using Goto.Controllers.Results;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Goto.Infrastructure.Converters;

public sealed class LinksetResultConverter : JsonConverter<LinksetResult>
{
    public override LinksetResult Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, LinksetResult value, JsonSerializerOptions options)
    {
        var linkset = value.Anchors.Select(a => {
            var dict = new Dictionary<string, object>
            {
                { "anchor", a.Anchor }
            };

            foreach (var linkType in a.Links.GroupBy(l => l.LinkType))
            {
                dict.Add(linkType.Key.Replace("gs1:", "https://ref.gs1.org/voc/"), linkType.Select(l => new
                {
                    l.Title,
                    l.Href,
                    l.Hreflang,
                    l.Type
                }));
            }

            if(a.Links.Count(l => l.IsDefault) > 1)
            {
                dict.Add("https://ref.gs1.org/voc/defaultLinkMulti", a.Links.Where(l => l.IsDefault).Select(l => new
                {
                    l.Title,
                    l.Href,
                    l.Hreflang,
                    l.Type
                }));
            }

            var defaultLink = a.Links.First(l => l.IsDefault);
            dict["https://ref.gs1.org/voc/defaultLink"] = new[]{ new
            {
                defaultLink.Title,
                defaultLink.Href,
            }};

            return dict;
        });

        JsonSerializer.Serialize(writer, new { linkset }, options);
    }
}