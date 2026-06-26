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
        public void ShouldReturnExpectedMetadata()
        {
            var result = Controller.GetMetadataInformation(new Uri("https://test.url/.well-known/gs1resolver"));

            var okObjectResult = Assert.IsInstanceOfType<OkObjectResult>(result);
            var metadata = Assert.IsInstanceOfType<MetadataResult>(okObjectResult.Value);

            Assert.AreEqual("https://test.url", metadata.ResolverRoot);
            Assert.AreEqual("GOTO", metadata.Name);
            Assert.IsTrue(metadata.LinkTypeDefaultCanBeLinkset);
            Assert.AreEqual("GOTO", metadata.Contact.Fn);
            CollectionAssert.AreEquivalent(new[] { "all" }, metadata.SupportedPrimaryKeys);
        }
    }

    #endregion

    #region ResolveLinkset

    [TestClass]
    public sealed class ResolveLinkset()
    {
        public static Context Context { get; private set; } = null!;
        public static ApiTimeProvider TimeProvider { get; private set; } = null!;

        [ClassInitialize]
        public static void Initialize(TestContext testContext)
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseSqlite($"Data Source={nameof(ResolveLinkset)}.db")
                .Options;

            TimeProvider = new ApiTimeProvider();
            Context = new Context(options, TimeProvider);

            Context.Database.EnsureDeleted();
            Context.Database.EnsureCreated();

            Context.Anchors.Add(new Anchor
            {
                Prefix = "01",
                CompanyPrefix = "123456",
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
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            if (Context is not null)
            {
                Context.Database.EnsureDeleted();
            }
        }

        [TestMethod]
        public void ShouldReturnStatusCodeNotFoundWhenNoLinkIsConfigured()
        {
            var digitalLink = new DigitalLink() { AIs = [new KeyValue { Key = new() { Type = AIType.PrimaryKey, Code = "01" }, Value = "undefined", Issues = [] }], CompanyPrefix = "654321", HostUrl = "https://test.com", QueryString = [] };
            var result = Controller.ResolveLinkset(digitalLink, Context);

            Assert.IsInstanceOfType<NotFoundObjectResult>(result);
        }

        [TestMethod]
        public void ShouldReturnStatusCodeOkWhenLinksAreConfigured()
        {
            var digitalLink = new DigitalLink() { AIs = [new KeyValue { Key = new() { Type = AIType.PrimaryKey, Code = "01" }, Value = "123456789", Issues = [] }], CompanyPrefix = "123456", HostUrl = "https://test.com", QueryString = [] };
            var result = Controller.ResolveLinkset(digitalLink, Context);

            Assert.IsInstanceOfType<OkObjectResult>(result);
        }
    }

    #endregion
}
