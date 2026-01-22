namespace Gs1DigitalLink.Api.Contracts;

public sealed record DecompressionRequest
{
    public required string DigitalLink { get; init; }
}
