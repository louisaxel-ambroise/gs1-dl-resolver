using Microsoft.Extensions.Localization;
using System.Reflection;

namespace Goto.Services.Translations;

public sealed class CommonLocalizationService(IStringLocalizerFactory factory)
{
    private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
    private static readonly string ResourceName = "Services.Translations.Resources";

    private readonly IStringLocalizer _localizer = factory.Create(ResourceName, Assembly.GetName().Name!);

    public string this[string key] => _localizer[key];
}
