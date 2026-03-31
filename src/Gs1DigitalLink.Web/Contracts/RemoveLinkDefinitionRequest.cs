using System.ComponentModel.DataAnnotations;

namespace Gs1DigitalLink.Web.Contracts;

public sealed record RemoveLinkDefinitionRequest
{
    [Required]
    public required string Prefix { get; init; }
    [Required, MinLength(1)]
    public required string[] LinkTypes { get; init; }
    public required string? Language { get; init; }
}
