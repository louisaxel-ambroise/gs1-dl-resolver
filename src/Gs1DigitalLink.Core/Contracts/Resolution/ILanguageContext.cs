using Gs1DigitalLink.Core.Services.Resolution;

namespace Gs1DigitalLink.Core.Contracts.Resolution;

public interface ILanguageContext
{
    IEnumerable<LanguagePreference> GetLanguages();
}
