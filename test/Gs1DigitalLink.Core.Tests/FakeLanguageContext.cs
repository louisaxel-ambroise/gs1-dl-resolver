using Gs1DigitalLink.Core.Services.Resolution;

namespace Gs1DigitalLink.Core.Tests;

internal sealed class FakeLanguageContext(params string[] languages) : ILanguageContext
{
    public IEnumerable<LanguagePreference> GetLanguages()
        => [.. languages.Select(l => new LanguagePreference { Language = l, Region = null, Quality = 1.0})];
}