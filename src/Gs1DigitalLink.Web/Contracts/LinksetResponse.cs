using Gs1DigitalLink.Web.Formatters.Json;
using System.Text.Json.Serialization;

namespace Gs1DigitalLink.Web.Contracts;

[JsonConverter(typeof(LinksetJsonConverter))]
public sealed record LinksetResponse(string ResolverRoot, string Anchor, IDictionary<string, IEnumerable<LinkDefinition>> Links);
