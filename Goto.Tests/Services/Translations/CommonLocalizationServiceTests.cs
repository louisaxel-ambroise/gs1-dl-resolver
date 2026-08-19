using Goto.Services.Translations;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace Goto.Tests.Services.Translations;

[TestClass]
public class CommonLocalizationServiceTests
{
    [TestMethod]
    public void ShouldLoadTheResourcesCorrectly()
    {
        var stringLocalizerFactory = new ResourceManagerStringLocalizerFactory(Options.Create<LocalizationOptions>(new ()), new LoggerFactory());
        var localizationService = new CommonLocalizationService(stringLocalizerFactory);
        CultureInfo.CurrentUICulture = new("en-GB");

        var translation = localizationService["gs1:pip"];

        Assert.AreEqual("Product Information", translation);
    }

    [TestMethod]
    public void ShouldTranslateUsingCurrentCulture()
    {
        var stringLocalizerFactory = new ResourceManagerStringLocalizerFactory(Options.Create<LocalizationOptions>(new()), new LoggerFactory());
        var localizationService = new CommonLocalizationService(stringLocalizerFactory);
        CultureInfo.CurrentUICulture = new("fr-BE");

        var translation = localizationService["gs1:pip"];

        Assert.AreEqual("Infos sur le produit", translation);
    }
}
