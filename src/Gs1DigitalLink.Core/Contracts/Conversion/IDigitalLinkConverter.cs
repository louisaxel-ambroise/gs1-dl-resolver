using Gs1DigitalLink.Core.Services.Conversion;

namespace Gs1DigitalLink.Core.Contracts.Conversion;

public interface IDigitalLinkConverter
{
    DigitalLink Parse(string digitalLink);
}

public interface IDigitalLinkPrefixConverter
{
    Identifier Parse(string input);
}
