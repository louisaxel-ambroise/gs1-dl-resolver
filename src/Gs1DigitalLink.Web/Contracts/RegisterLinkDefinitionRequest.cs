using System.ComponentModel.DataAnnotations;

namespace Gs1DigitalLink.Web.Contracts;

public sealed record RegisterLinkDefinitionRequest
{
    [Required]
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

public sealed record RegisterLinkApplicability : IValidatableObject
{
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (To is not null && From >= To)
        {
            yield return new ValidationResult("'To' must be later than 'From'.", [nameof(To)]);
        }
    }
}