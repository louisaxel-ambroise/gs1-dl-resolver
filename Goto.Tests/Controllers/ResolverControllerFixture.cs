using Goto.Controllers;
using Goto.Controllers.Results;
using Goto.Data;
using Goto.Data.Entities;
using Goto.Services;
using Goto.Services.Conversion;
using Goto.Services.Conversion.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Goto.Tests.Controllers;

public static class ResolverControllerFixture
{
    public static ResolverController Controller { get; set; } = new();

    #region GetMetadataInformation

    [TestClass]
    public sealed class GetMetadataInformation()
    {
        [TestMethod]
        public void ResultShouldBeOkObjectResult()
        {
            var result = Controller.GetMetadataInformation(new Uri("https://test.url/.well-known/gs1resolver"));

            Assert.IsInstanceOfType<OkObjectResult>(result);
        }

        [TestMethod]
        public void ShouldReturnAMetadataResult()
        {
            var result = (OkObjectResult) Controller.GetMetadataInformation(new Uri("https://test.url/.well-known/gs1resolver"));

            Assert.IsInstanceOfType<MetadataResult>(result.Value);
        }
    }

    #endregion

    #region ResolveLinkset

    [TestClass]
    public sealed class ResolveLinkset()
    {
        public Context Context { get; private set; } = null!;
        public DigitalLink DigitalLink { get; private set; } = null!;
        public ApiTimeProvider TimeProvider { get; private set; } = null!;

        [TestInitialize]
        public void Initialize()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseSqlite("Data Source=:memory:")
                .Options;

            DigitalLink = new() { AIs = [new KeyValue { Key = new() { Type = AIType.PrimaryKey, Code = "01" }, Value = "123456789", Issues = [] }], CompanyPrefix = "", HostUrl = "https://test.com", QueryString = [] };
            TimeProvider = new ApiTimeProvider();
            Context = new Context(options, TimeProvider);

            Context.Database.EnsureDeleted();
            Context.Database.EnsureCreated();
        }

        [TestMethod]
        public void ShouldReturnStatusCodeNotFoundWhenNoLinkIsConfigured()
        {
            var result = Controller.ResolveLinkset(DigitalLink, Context);

            Assert.IsInstanceOfType<NotFoundObjectResult>(result);
        }

        [TestMethod]
        public void ShouldReturnStatusCodeOkWhenLinksAreConfigured()
        {
            Context.Anchors.Add(new Anchor
            {
                Prefix = "01/",
                Description = "test anchor",
                Links = [ new AnchorLink
                {
                    ActiveFrom = DateTimeOffset.MinValue,
                    ActiveUntil = DateTimeOffset.MaxValue,
                    Language = new("en-GB"),
                    LinkType = "gs1:pip",
                    RedirectUrl = "https://redirect.pip",
                    Title = "PIP redirection"
                } ]
            });
            Context.SaveChanges();

            var result = Controller.ResolveLinkset(DigitalLink, Context);

            Assert.IsInstanceOfType<OkObjectResult>(result);
        }
    }

    #endregion
}
