namespace Gs1DigitalLink.Core.Services.Resolution;

public interface ILanguageContext
{
    IEnumerable<LanguagePreference> GetLanguages();
}
