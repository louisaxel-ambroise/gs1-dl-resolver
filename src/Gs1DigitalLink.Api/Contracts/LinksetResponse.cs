using Gs1DigitalLink.Api.Formatters.Json;
using System.Text.Json.Serialization;

namespace Gs1DigitalLink.Api.Contracts;

[JsonConverter(typeof(LinksetJsonConverter))]
public sealed record LinksetResponse(string Anchor, IDictionary<string, IEnumerable<LinkDefinition>> Links);
