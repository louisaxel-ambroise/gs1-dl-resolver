namespace DigitalLinkToolkit.Conversion.Model;

public record Identifier
{
    public required string? CompanyPrefix { get; init; }
    public required string Value { get; init; }
}
