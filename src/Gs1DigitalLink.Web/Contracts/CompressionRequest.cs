using Gs1DigitalLink.Core.Services.Conversion;
using System.Text.Json.Serialization;
namespace Gs1DigitalLink.Web.Contracts;

public sealed record CompressionRequest
{
    public required string DigitalLink { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required DLCompressionType CompressionType { get; init; }
    public required bool CompressQueryString { get; init; }
}
