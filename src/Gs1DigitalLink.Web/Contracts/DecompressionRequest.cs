namespace Gs1DigitalLink.Web.Contracts;

public sealed record DecompressionRequest
{
    public required string DigitalLink { get; init; }
}
