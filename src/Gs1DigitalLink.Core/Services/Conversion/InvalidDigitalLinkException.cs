namespace Gs1DigitalLink.Core.Services.Conversion;

public sealed class InvalidDigitalLinkException(IEnumerable<ValidationIssue> issues) : Exception("The provided digital link is invalid")
{
    public IEnumerable<ValidationIssue> Issues => issues;
}
