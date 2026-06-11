using Microsoft.Extensions.Localization;
using System.Reflection;

namespace Goto.Translations;

public sealed class CommonLocalizationService(IStringLocalizerFactory factory)
{
    private const string Resources = nameof(Resources);
    private readonly IStringLocalizer _localizer = factory.Create(Resources, Assembly.GetExecutingAssembly().GetName().Name!);

    public string this[string key] => _localizer[key];
}
