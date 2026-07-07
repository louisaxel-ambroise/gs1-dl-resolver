using DigitalLinkToolkit.Conversion.DTOs;
using DigitalLinkToolkit.Conversion.Model;
using Goto.Services.Data;
using Goto.Services.Data.Entities;
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
            HostUrl = new("https://test.com"),
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
                Description = new("Test anchor 1"),
                Prefix = new("01/05414195535264"),
                CompanyPrefix = new("5414195"),
                Links =
                [
                    new AnchorLink
                    {
                        ActiveFrom = DateTimeOffset.MinValue,
                        ActiveUntil = DateTimeOffset.MaxValue,
                        Language = new("en-GB"),
                        LinkType = new("gs1:pip"),
                        RedirectUrl = new("https://test.url/en"),
                        Title = new("Test link 1"),
                        MediaType = new("text/html")
                    },
                    new AnchorLink
                    {
                        ActiveFrom = DateTimeOffset.MinValue,
                        ActiveUntil = DateTimeOffset.MaxValue,
                        Language = new("fr-BE"),
                        LinkType = new("gs1:pip"),
                        RedirectUrl = new("https://test.url/fr"),
                        Title = new("Lien de test 2"),
                        MediaType = new("text/html")
                    }
                ]
            };

            yield return new Anchor
            {
                Description = new("Test anchor 1"),
                Prefix = new("01"),
                CompanyPrefix = new("5414195"),
                Links =
                [
                    new AnchorLink
                    {
                        ActiveFrom = DateTimeOffset.MinValue,
                        ActiveUntil = DateTimeOffset.MaxValue,
                        Language = new("en-GB"),
                        LinkType = new("gs1:homepage"),
                        RedirectUrl = new("https://test.url/about-us"),
                        Title = new("Company description"),
                        MediaType = new("text/html"),
                        IsDefault = true
                    }
                ]
            };

            yield return new Anchor
            {
                Description = new("Test anchor 2"),
                Prefix = new("01/04047111050247"),
                CompanyPrefix = new("4047111"),
                Links =
                [
                    new AnchorLink
                    {
                        ActiveFrom = DateTimeOffset.MinValue,
                        ActiveUntil = DateTimeOffset.MaxValue,
                        Language = new("en-GB"),
                        LinkType = new("gs1:pip"),
                        RedirectUrl = new("https://test.url/en"),
                        Title = new("Test link 1"),
                        MediaType = new("text/html")
                    },
                    new AnchorLink
                    {
                        ActiveFrom = DateTimeOffset.MinValue,
                        ActiveUntil = DateTimeOffset.MaxValue,
                        Language = new("fr-BE"),
                        LinkType = new("gs1:pip"),
                        RedirectUrl = new("https://test.url/fr"),
                        Title = new("Lien de test 2"),
                        MediaType = new("text/html")
                    }
                ]
            };
        }

        public static IEnumerable<Insight> CreateInsights()
        {
            yield return new Insight
            {
                CompanyPrefix = new("5414195"),
                Accept = new("*/*"),
                AcceptLanguage = new("en-GB"),
                LinkCount = 1,
                StatusCode = 307,
                RecordDate = DateTimeOffset.Parse("2026-05-03T15:53:20"),
                RequestDate = DateTimeOffset.Parse("2026-05-03T15:53:20"),
                DigitalLink = new("01/05414195535264"),
                Url = new("01/05414195535264")
            };

            yield return new Insight
            {
                CompanyPrefix = new("5414195"),
                Accept = new("image/png"),
                AcceptLanguage = new("fr-BE, de-DE;q=0.7"),
                LinkCount = 0,
                StatusCode = 404,
                RecordDate = DateTimeOffset.Parse("2026-05-03T15:54:32"),
                RequestDate = DateTimeOffset.Parse("2026-05-03T15:54:30"),
                DigitalLink = new("01/05414195535264"),
                Url = new("01/05414195535264")
            };

            yield return new Insight
            {
                CompanyPrefix = new("5414195"),
                Accept = new("*/*"),
                AcceptLanguage = new("en-GB"),
                LinkCount = 1,
                StatusCode = 307,
                RecordDate = DateTimeOffset.Parse("2026-05-10T03:11:34"),
                RequestDate = DateTimeOffset.Parse("2026-05-10T03:11:20"),
                DigitalLink = new("01/05414195535264/10/XYZ"),
                Url = new("01/05414195535264/10/XYZ")
            };
        }
    }
}
