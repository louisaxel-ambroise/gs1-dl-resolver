using Gs1DigitalLink.Core.Model;
using Gs1DigitalLink.Core.Model.Interfaces;
using Gs1DigitalLink.Core.Services.Conversion;
using Gs1DigitalLink.Core.Services.Conversion.Utils;
using Gs1DigitalLink.Core.Services.Resolution;

namespace Gs1DigitalLink.Core.Tests.Resolution;

[TestClass]
public sealed class DigitalLinkResolverTests
{
    private static DigitalLink CreateDigitalLink()
    {
        return new DigitalLink
        {
            CompanyPrefix = "950600",
            QueryString = [],
            AIs =
            [
                new KeyValue
                { 
                    Key = new Services.Conversion.Utils.Identifier { Code = "01", Type = AIType.PrimaryKey }, 
                    Value = "09506000134352",
                    Issues = []
                },
                new KeyValue
                {
                    Key = new Services.Conversion.Utils.Identifier { Code = "10", Type = AIType.Qualifier },
                    Value = "ABC123",
                    Issues = []
                }
            ]
        };
    }

    [TestMethod]
    public void GetCandidatesTests_ShouldReturnConfiguredLinks()
    {
        var prefix = new Prefix("950600", "01/09506000134352");
        prefix.AddLink(new Link
        {
            Title = "test",
            LinkType = "gs1:defaultLink",
            Language = "en",
            RedirectUrl = "http://a.com",
            Availability = new(DateTimeOffset.MinValue, DateTimeOffset.MaxValue)
        });

        var context = new ResolverContext(new FakeEventDispatcher(), TimeProvider.System);
        var resolver = new DigitalLinkResolver(context, new FakeLanguageContext());
        context.Database.EnsureCreated();
        context.Prefixes.Add(prefix);
        context.SaveChanges();

        var result = resolver.ResolveLinkType(CreateDigitalLink(), TimeProvider.System.GetUtcNow(), null);

        Assert.ContainsSingle(result.Links);
        Assert.AreEqual("http://a.com", result.Links.First().RedirectUrl);
    }
}

internal class FakeEventDispatcher : IEventDispatcher
{
    public void Dispatch(IDomainEvent domainEvent)
    {
    }
}