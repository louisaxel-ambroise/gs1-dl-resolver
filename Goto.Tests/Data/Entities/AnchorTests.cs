using Goto.Data.Entities;

namespace Goto.Tests.Data.Entities;

[TestClass]
public class AnchorTests
{
    public static readonly Anchor Sut = new()
    {
        CompanyPrefix = "123456",
        Description = "test 123456",
        Prefix = "01/123456789",
        Links =
            [
                new ()
                {
                    ActiveFrom = DateTimeOffset.MinValue,
                    ActiveUntil = DateTimeOffset.MaxValue,
                    Language = new("en-GB"),
                    LinkType = "gs1:pip",
                    RedirectUrl = "https://test.com/1",
                    Title = "Test link",
                    MediaType = "image/png"
                },
                new ()
                {
                    ActiveFrom = DateTimeOffset.MinValue,
                    ActiveUntil = DateTimeOffset.MaxValue,
                    Language = new("en-US"),
                    LinkType = "gs1:pip",
                    RedirectUrl = "https://test.com/2",
                    Title = "Test link",
                    MediaType = "text/html"
                },
                new ()
                {
                    ActiveFrom = DateTimeOffset.MinValue,
                    ActiveUntil = DateTimeOffset.MaxValue,
                    Language = new("fr-BE"),
                    LinkType = "gs1:pip",
                    RedirectUrl = "https://test.com/3",
                    Title = "Test link",
                    MediaType = "image/png"
                },
                new ()
                {
                    ActiveFrom = DateTimeOffset.MinValue,
                    ActiveUntil = DateTimeOffset.MaxValue,
                    Language = new("de"),
                    LinkType = "gs1:homepage",
                    RedirectUrl = "https://test.com/4",
                    Title = "Test link",
                    MediaType = "text/html"
                },
                new ()
                {
                    ActiveFrom = DateTimeOffset.MinValue,
                    ActiveUntil = DateTimeOffset.MaxValue,
                    Language = new("ar"),
                    LinkType = "gs1:consumerHandlingStorageInfo",
                    RedirectUrl = "https://test.com/5",
                    Title = "Test link",
                    MediaType = "image/png",
                    IsDefault = true
                }
            ]
    };

    [TestMethod]
    public void FindBestMatchShouldReturnTheLinksThatMatchLinkTypeAndLanguageAndMediaType()
    {
        var result = Sut.FindBestMatches("gs1:pip", [new("en-US")], ["image/png"]);

        Assert.HasCount(1, result);
        CollectionAssert.AreEquivalent(new[] { Sut.Links[1] }, result);
    }

    [TestMethod]
    public void FindBestMatchShouldReturnAnEmptyListIfNoneMatchesLinkType()
    {
        var result = Sut.FindBestMatches("gs1:relatedVideo", [new("hr-HR")], ["application/json"]);

        Assert.HasCount(0, result);
    }

    [TestMethod]
    public void FindBestMatchShouldReturnTheLinksThatMatchLinkTypeAndMediaTypeIfNoneMatchesLanguage()
    {
        var result = Sut.FindBestMatches("gs1:pip", [new("hr-HR")], ["image/png"]);

        Assert.HasCount(2, result);
        CollectionAssert.AreEquivalent(new[] { Sut.Links[0], Sut.Links[2] }, result);
    }

    [TestMethod]
    public void FindBestMatchShouldReturnTheLinksThatMatchLinkTypeAndLanguageIfNoneMatchesMediaType()
    {
        var result = Sut.FindBestMatches("gs1:pip", [new("en")], ["application/json"]);

        Assert.HasCount(2, result);
        CollectionAssert.AreEquivalent(new[] { Sut.Links[0], Sut.Links[1] }, result);
    }

    [TestMethod]
    public void FindBestMatchShouldReturnTheDefaultLinksWhenLinkTypeIsEmpty()
    {
        var result = Sut.FindBestMatches([new("hr-HR")], ["image/png"]);

        Assert.HasCount(1, result);
        CollectionAssert.AreEquivalent(new[] { Sut.Links[4] }, result);
    }
}
