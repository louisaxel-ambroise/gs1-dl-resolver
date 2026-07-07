using Goto.Services.Data.Entities;

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
                    Id = 2,
                    ActiveFrom = DateTimeOffset.MinValue,
                    ActiveUntil = DateTimeOffset.MaxValue,
                    Language = new("en-GB"),
                    LinkType = new("gs1:pip"),
                    RedirectUrl = "https://test.com/1",
                    Title = "Test link",
                    MediaType = new("image/png")
                },
                new ()
                {
                    Id = 1,
                    ActiveFrom = DateTimeOffset.MinValue,
                    ActiveUntil = DateTimeOffset.MaxValue,
                    Language = new("en-US"),
                    LinkType = new("gs1:pip"),
                    RedirectUrl = "https://test.com/2",
                    Title = "Test link",
                    MediaType = new("text/html")
                },
                new ()
                {                    
                    Id = 3,
                    ActiveFrom = DateTimeOffset.MinValue,
                    ActiveUntil = DateTimeOffset.MaxValue,
                    Language = new("fr-BE"),
                    LinkType = new("gs1:pip"),
                    RedirectUrl = "https://test.com/3",
                    Title = "Test link",
                    MediaType = new("image/png")
                },
                new ()
                {
                    Id = 4,
                    ActiveFrom = DateTimeOffset.MinValue,
                    ActiveUntil = DateTimeOffset.MaxValue,
                    Language = new("de"),
                    LinkType = new("gs1:homepage"),
                    RedirectUrl = "https://test.com/4",
                    Title = "Test link",
                    MediaType = new("text/html")
                },
                new ()
                {
                    Id = 5,
                    ActiveFrom = DateTimeOffset.MinValue,
                    ActiveUntil = DateTimeOffset.MaxValue,
                    Language = new("ar"),
                    LinkType = new("gs1:consumerHandlingStorageInfo"),
                    RedirectUrl = "https://test.com/5",
                    Title = "Test link",
                    MediaType = new("image/png"),
                    IsDefault = true
                }
            ]
    };

    [TestMethod]
    public void FindBestMatchShouldReturnTheLinksThatMatchLinkTypeAndLanguageAndMediaType()
    {
        var result = Sut.FindBestMatches([new("en-US")], [new("image/png")]);

        Assert.HasCount(1, result);
        CollectionAssert.AreEquivalent(new[] { Sut.Links[1] }, result);
    }

    [TestMethod]
    public void FindBestMatchShouldReturnAnEmptyListIfNoneMatchesLinkType()
    {
        var result = Sut.FindBestMatches([new("hr-HR")], [new("application/json")]);

        Assert.HasCount(0, result);
    }

    [TestMethod]
    public void FindBestMatchShouldReturnTheLinksThatMatchLinkTypeAndMediaTypeIfNoneMatchesLanguage()
    {
        var result = Sut.FindBestMatches([new("hr-HR")], [new("image/png")]);

        Assert.HasCount(2, result);
        CollectionAssert.AreEquivalent(new[] { Sut.Links[0], Sut.Links[2] }, result);
    }

    [TestMethod]
    public void FindBestMatchShouldReturnTheLinksThatMatchLinkTypeAndLanguageIfNoneMatchesMediaType()
    {
        var result = Sut.FindBestMatches([new("en-US")], [new("application/json")]);

        Assert.HasCount(2, result);
        CollectionAssert.AreEquivalent(new[] { Sut.Links[0], Sut.Links[1] }, result);
        Assert.AreSame(Sut.Links[1], result[0]);
    }

    [TestMethod]
    public void FindBestMatchShouldOrderTheResultsByIdAscending()
    {
        var result = Sut.FindBestMatches([new("en-US")], [new("application/json")]);

        Assert.HasCount(2, result);
        Assert.AreSame(Sut.Links[1], result[0]);
    }

    [TestMethod]
    public void FindBestMatchShouldReturnTheDefaultLinksWhenLinkTypeIsEmpty()
    {
        var result = Sut.FindBestMatches([new("hr-HR")], [new("image/png")]);

        Assert.HasCount(1, result);
        CollectionAssert.AreEquivalent(new[] { Sut.Links[4] }, result);
    }
}
