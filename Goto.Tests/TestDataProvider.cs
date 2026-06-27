using Goto.Data;
using Goto.Data.Entities;
using Goto.Services.Conversion;
using Goto.Services.Conversion.Utils;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Goto.Tests;

public static class TestDataProvider
{
    internal static Context CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<Context>().UseSqlite("Data Source=:memory:");
        var context = new Context(optionsBuilder.Options, new());

        context.Database.OpenConnection();
        context.Database.EnsureCreated();

        context.AddRange(TestDataFactory.CreateAnchors());
        context.AddRange(TestDataFactory.CreateInsights());
        context.SaveChanges();

        return context;
    }

    internal static DigitalLink CreateDigitalLink(string representation, string companyPrefix)
    {
        var parts = representation.Split('/');
        var keyValues = new List<KeyValue>();

        for (var i = 0; i < parts.Length; i += 2)
        {
            keyValues.Add(new()
            {
                Key = new (){ Type = i == 0 ? AIType.PrimaryKey : AIType.Qualifier, Code = parts[i] },
                Value = parts[i + 1],
                Issues = []
            });
        }

        return new DigitalLink
        {
            CompanyPrefix = companyPrefix,
            HostUrl = "https://test.com",
            QueryString = [],
            AIs = keyValues
        };
    }

    internal static ClaimsPrincipal CreateUserPrincipal(string companyPrefix)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "Test Generated User"),
            new Claim("gs1:gcp", companyPrefix)
        };
        var identity = new ClaimsIdentity(claims, "Test.Key");

        return  new ClaimsPrincipal(identity);
    }

    private static class TestDataFactory
    {
        public static IEnumerable<Anchor> CreateAnchors()
        {
            yield return new Anchor
            {
                Description = "Test anchor 1",
                Prefix = "01/05414195535264",
                CompanyPrefix = "5414195",
                Links = new[]
                {
                    new AnchorLink
                    {
                        ActiveFrom = DateTimeOffset.MinValue,
                        ActiveUntil = DateTimeOffset.MaxValue,
                        Language = new("en-GB"),
                        LinkType = "gs1:pip",
                        RedirectUrl = "https://test.url/en",
                        Title = "Test link 1",
                        MediaType = "text/html"
                    },
                    new AnchorLink
                    {
                        ActiveFrom = DateTimeOffset.MinValue,
                        ActiveUntil = DateTimeOffset.MaxValue,
                        Language = new("fr-BE"),
                        LinkType = "gs1:pip",
                        RedirectUrl = "https://test.url/fr",
                        Title = "Lien de test 2",
                        MediaType = "text/html"
                    }
                }.ToList()
            };

            yield return new Anchor
            {
                Description = "Test anchor 1",
                Prefix = "01",
                CompanyPrefix = "5414195",
                Links = new[]
                {
                    new AnchorLink
                    {
                        ActiveFrom = DateTimeOffset.MinValue,
                        ActiveUntil = DateTimeOffset.MaxValue,
                        Language = new("en-GB"),
                        LinkType = "gs1:homepage",
                        RedirectUrl = "https://test.url/about-us",
                        Title = "Company description",
                        MediaType = "text/html",
                        IsDefault = true
                    }
                }.ToList()
            };

            yield return new Anchor
            {
                Description = "Test anchor 2",
                Prefix = "01/04047111050247",
                CompanyPrefix = "4047111",
                Links = new[]
                {
                    new AnchorLink
                    {
                        ActiveFrom = DateTimeOffset.MinValue,
                        ActiveUntil = DateTimeOffset.MaxValue,
                        Language = new("en-GB"),
                        LinkType = "gs1:pip",
                        RedirectUrl = "https://test.url/en",
                        Title = "Test link 1",
                        MediaType = "text/html"
                    },
                    new AnchorLink
                    {
                        ActiveFrom = DateTimeOffset.MinValue,
                        ActiveUntil = DateTimeOffset.MaxValue,
                        Language = new("fr-BE"),
                        LinkType = "gs1:pip",
                        RedirectUrl = "https://test.url/fr",
                        Title = "Lien de test 2",
                        MediaType = "text/html"
                    }
                }.ToList()
            };
        }

        public static IEnumerable<Insight> CreateInsights()
        {
            yield return new Insight
            {
                CompanyPrefix = "5414195",
                Accept = "*/*",
                AcceptLanguage = "en-GB",
                LinkCount = 1,
                StatusCode = 307,
                QueryString = "",
                RecordDate = DateTimeOffset.Parse("2026-05-03T15:53:20"),
                RequestDate = DateTimeOffset.Parse("2026-05-03T15:53:20"),
                Url = "01/05414195535264"
            };

            yield return new Insight
            {
                CompanyPrefix = "5414195",
                Accept = "*/*",
                AcceptLanguage = "en-GB",
                LinkCount = 1,
                StatusCode = 307,
                QueryString = "",
                RecordDate = DateTimeOffset.Parse("2026-05-10T03:11:34"),
                RequestDate = DateTimeOffset.Parse("2026-05-10T03:11:20"),
                Url = "01/05414195535264/10/XYZ"
            };
        }
    }
}
