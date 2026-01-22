namespace Gs1DigitalLink.Core.Services.Resolution;

public sealed record LanguagePreference
{
    public required string Language { get; set; }
    public required string? Region { get; set; }
    public required double Quality { get; set; }

    public override string ToString()
    {
        return string.IsNullOrEmpty(Region)
            ? Language
            : string.Join('-', Language, Region);
    }
}