using Goto.Services.Translations;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Goto.Tests.Translations;

[TestClass]
public class CommonLocalizationServiceTests
{
    [TestMethod]
    public void ShouldLoadTheResourcesCorrectly()
    {
        var stringLocalizerFactory = new ResourceManagerStringLocalizerFactory(Options.Create<LocalizationOptions>(new ()), new LoggerFactory());
        var localizationService = new CommonLocalizationService(stringLocalizerFactory);

        var translation = localizationService["gs1:pip"];

        Assert.AreEqual("Product Information", translation);
    }
}
