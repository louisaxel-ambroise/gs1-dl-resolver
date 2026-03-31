using Gs1DigitalLink.Core.Services.Conversion;
using Gs1DigitalLink.Core.Services.Resolution;

namespace Gs1DigitalLink.Core.Contracts.Resolution;

public interface IDigitalLinkResolver
{
    IResolutionResult ResolveLinkSet(DigitalLink digitalLink, DateTimeOffset applicability);
    IResolutionResult ResolveLinkType(DigitalLink digitalLink, DateTimeOffset applicability, string? linkType);
}