using System.ComponentModel.DataAnnotations;

namespace Gs1DigitalLink.Api.Contracts;

public sealed record RemoveLinkDefinitionRequest
{
    [Required, MinLength(3)]
    public required string Prefix { get; init; }
    [Required, MinLength(1)]
    public required string[] LinkTypes { get; init; }
    public required string? Language { get; init; }
}

public sealed record RegisterLinkDefinitionRequest
{
    [Required, MinLength(3)]
    public required string Prefix { get; init; }
    [Required, Url]
    public required string RedirectUrl { get; init; }
    [Required, MinLength(1)]
    public required string Title { get; init; }
    [Required, MinLength(1)]
    public required string[] LinkTypes { get; init; }
    public required string? Language { get; init; }
    public RegisterLinkApplicability? Applicability { get; init; }
}

public sealed record RegisterLinkApplicability
{
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
}