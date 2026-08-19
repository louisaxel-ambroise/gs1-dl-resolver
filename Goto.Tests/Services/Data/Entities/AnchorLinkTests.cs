using Goto.Services.Data.Entities;
using Goto.Services.Data.Enums;

namespace Goto.Tests.Services.Data.Entities;

[TestClass]
public class AnchorLinkTests
{
    #region Matches methods

    [TestMethod]
    [DataRow("en-GB", Match.FullMatch)]
    [DataRow("en", Match.WildcardMatch)]
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
            LinkType = new("gs1:pip"),
            MediaType = new("text/html"),
            RedirectUrl = "https://test.com",
            Title = "Test link"
        };

        var result = sut.MatchesLanguage(new(language));

        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    [DataRow("en-GB", Match.WildcardMatch)]
    [DataRow("en", Match.FullMatch)]
    [DataRow("en-US", Match.WildcardMatch)]
    [DataRow("en-AU", Match.WildcardMatch)]
    [DataRow("fr", Match.NoMatch)]
    [DataRow("de-DE", Match.NoMatch)]
    public void MatchesLanguageShouldReturnTheCorrectValueWhenRegionIsNotSet(string language, Match expectedResult)
    {
        var sut = new AnchorLink()
        {
            ActiveFrom = DateTimeOffset.MinValue,
            ActiveUntil = DateTimeOffset.MaxValue,
            Language = new("en"),
            LinkType = new("gs1:pip"),
            MediaType = new("text/html"),
            RedirectUrl = "https://test.com",
            Title = "Test link"
        };

        var result = sut.MatchesLanguage(new(language));

        Assert.AreEqual(expectedResult, result);
    }

    #endregion

    #region SetUnavailabilityDate

    [TestMethod]
    public void ShouldUpdateEndAvailabilityDate()
    {
        var now = DateTimeOffset.UtcNow;
        var sut = new AnchorLink()
        {
            ActiveFrom = DateTimeOffset.MinValue,
            ActiveUntil = DateTimeOffset.MaxValue,
            Language = new("en"),
            LinkType = new("gs1:pip"),
            MediaType = new("text/html"),
            RedirectUrl = "https://test.com",
            Title = "Test link"
        };

        sut.SetUnavailabilityDate(now);

        Assert.AreEqual(now, sut.ActiveUntil);
    }

    [TestMethod]
    public void ShouldRaiseErrorWhenEndAvailabilityDateIsBeforeTheSpecifiedDate()
    {
        var now = DateTimeOffset.UtcNow;
        var sut = new AnchorLink()
        {
            ActiveFrom = DateTimeOffset.MinValue,
            ActiveUntil = now.AddDays(-2),
            Language = new("en"),
            LinkType = new("gs1:pip"),
            MediaType = new("text/html"),
            RedirectUrl = "https://test.com",
            Title = "Test link"
        };

        Assert.Throws<InvalidOperationException>(() => sut.SetUnavailabilityDate(now));
    }

    #endregion
}
