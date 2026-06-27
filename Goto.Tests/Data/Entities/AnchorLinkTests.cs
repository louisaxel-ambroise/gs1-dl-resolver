using Goto.Data.Entities;
using Goto.Data.Enums;

namespace Goto.Tests.Data.Entities;

[TestClass]
public class AnchorLinkTests
{
    [TestMethod]
    [DataRow("en-GB", Match.FullMatch)]
    [DataRow("en", Match.FullMatch)]
    [DataRow("en-US", Match.PartialMatch)]
    [DataRow("en-AU", Match.PartialMatch)]
    [DataRow("fr", Match.NoMatch)]
    [DataRow("de-DE", Match.NoMatch)]
    public void MatchesLanguageShouldReturnTheCorrectValueWhenRegionIsSet(string language, Match expectedResult)
    {
        var sut = new AnchorLink()
        {
            ActiveFrom = DateTimeOffset.MinValue,
            ActiveUntil = DateTimeOffset.MaxValue,
            Language = new("en-GB"),
            LinkType = "gs1:pip",
            RedirectUrl = "https://test.com",
            Title = "Test link"
        };

        var result = sut.MatchesLanguage(new(language));

        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    [DataRow("en-GB", Match.FullMatch)]
    [DataRow("en", Match.FullMatch)]
    [DataRow("en-US", Match.FullMatch)]
    [DataRow("en-AU", Match.FullMatch)]
    [DataRow("fr", Match.NoMatch)]
    [DataRow("de-DE", Match.NoMatch)]
    public void MatchesLanguageShouldReturnTheCorrectValueWhenRegionIsNotSet(string language, Match expectedResult)
    {
        var sut = new AnchorLink()
        {
            ActiveFrom = DateTimeOffset.MinValue,
            ActiveUntil = DateTimeOffset.MaxValue,
            Language = new("en"),
            LinkType = "gs1:pip",
            RedirectUrl = "https://test.com",
            Title = "Test link"
        };

        var result = sut.MatchesLanguage(new(language));

        Assert.AreEqual(expectedResult, result);
    }
}
