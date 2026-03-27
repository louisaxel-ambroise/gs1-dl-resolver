using Gs1DigitalLink.Core.Services.Conversion;

namespace Gs1DigitalLink.Core.Model.Exceptions;

public sealed class InvalidDigitalLinkException(IEnumerable<ValidationIssue> issues) : Exception("The provided digital link is invalid")
{
    public IEnumerable<ValidationIssue> Issues => issues;
}
