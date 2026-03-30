using Gs1DigitalLink.Core.Model;
using Gs1DigitalLink.Core.Services.Conversion;

namespace Gs1DigitalLink.Core.Contracts.Registration;

public interface ILinkRegistrator
{
    void RegisterLink(Identifier prefix, string redirectUrl, string title, Language? language, DateRange applicability, IEnumerable<string> linkTypes);
    void DeleteLink(Identifier prefix, Language? language, IEnumerable<string> linkTypes);
}