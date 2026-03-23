using System.ComponentModel.DataAnnotations;

namespace Gs1DigitalLink.Web.Contracts.Validation;

public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var timeProvider = validationContext.GetRequiredService<TimeProvider>();

        return value is DateTimeOffset dateTimeOffset && dateTimeOffset > timeProvider.GetUtcNow()
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessage ?? "The date must be in the future.");
    }
}