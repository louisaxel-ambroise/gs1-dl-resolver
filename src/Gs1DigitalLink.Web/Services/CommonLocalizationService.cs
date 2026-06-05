using Microsoft.Extensions.Localization;
using System.Reflection;

namespace Gs1DigitalLink.Web.Services;

public sealed class CommonLocalizationService
{
    private const string CommonResources = nameof(CommonResources);
    private readonly IStringLocalizer _localizer;

    public CommonLocalizationService(IStringLocalizerFactory factory)
    {
        var assemblyName = Assembly.GetExecutingAssembly().GetName();
        _localizer = factory.Create(CommonResources, assemblyName.Name!);
    }

    public string this[string key] => _localizer[key];
}