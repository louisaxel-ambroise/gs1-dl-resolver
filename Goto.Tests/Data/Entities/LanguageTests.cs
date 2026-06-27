using Goto.Data.Entities;
using Goto.Data.Enums;

namespace Goto.Tests.Data.Entities;

[TestClass]
public class LanguageTests
{
    [TestMethod]
    [DataRow("en-GB", Match.FullMatch)]
    [DataRow("en", Match.FullMatch)]
    [DataRow("en-US", Match.PartialMatch)]
    [DataRow("en-AU", Match.PartialMatch)]
    [DataRow("fr", Match.NoMatch)]
    [DataRow("de-DE", Match.NoMatch)]
    public void MatchesShouldReturnTheCorrectValueWhenRegionIsSet(string language, Match expectedResult)
    {
        var sut = new Language("en-GB");
        var result = sut.Matches(new(language));

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
        var sut = new Language("en");
        var result = sut.Matches(new(language));

        Assert.AreEqual(expectedResult, result);
    }
}
