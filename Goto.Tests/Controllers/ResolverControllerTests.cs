using DigitalLinkToolkit.Conversion.DTOs;
using Goto.Controllers;
using Goto.Controllers.Results;
using DigitalLinkToolkit.Conversion.Model;
using Microsoft.AspNetCore.Mvc;
using Goto.Infrastructure.Results;

namespace Goto.Tests.Controllers;

public static class ResolverControllerTests
{
    public static ResolverController Controller { get; set; } = new();

    #region ResolveLinkset

    [TestClass]
    public sealed class ResolveLinkset
    {
        [TestMethod]
        public void ShouldReturnStatusCodeNotFoundWhenNoLinkIsConfigured()
        {
            var context = TestDataProvider.CreateContext();
            var digitalLink = new DigitalLink() { AIs = [new KeyValue { Key = new() { Type = AIType.PrimaryKey, Code = "01" }, Value = "undefined", Issues = [] }], CompanyPrefix = "654321", HostUrl = "https://test.com", QueryString = [] };
            var result = Controller.ResolveLinkset(digitalLink, context);

            Assert.IsInstanceOfType<NotFoundObjectResult>(result);
        }

        [TestMethod]
        public void ShouldReturnTheLinksetWhenLinksAreConfigured()
        {
            var context = TestDataProvider.CreateContext();
            var digitalLink = TestDataProvider.CreateDigitalLink("01/05414195535264/10/ABC", "5414195");
            var result = Controller.ResolveLinkset(digitalLink, context);

            var okObjectResult = Assert.IsInstanceOfType<OkObjectResult>(result);
            var linksetResult = Assert.IsInstanceOfType<LinksetResult>(okObjectResult.Value);

            Assert.AreEqual(2, linksetResult.Anchors.Count());
        }
    }

    #endregion

    #region ResolveLinkType

    [TestClass]
    public sealed class ResolveLinkType
    {
        [TestMethod]
        public void ShouldReturnStatusCodeNotFoundWhenNoLinkIsConfigured()
        {
            var context = TestDataProvider.CreateContext();
            var digitalLink = new DigitalLink() { AIs = [new KeyValue { Key = new() { Type = AIType.PrimaryKey, Code = "01" }, Value = "undefined", Issues = [] }], CompanyPrefix = "654321", HostUrl = "https://test.com", QueryString = [] };
            var result = Controller.ResolveLinkType(digitalLink, new("gs1:pip"), [new("*/*")], [new("en-GB")], context);

            Assert.IsInstanceOfType<NotFoundObjectResult>(result);
        }

        [TestMethod]
        public void ShouldRedirectToBestMatchIfItExists()
        {
            var context = TestDataProvider.CreateContext();
            var digitalLink = TestDataProvider.CreateDigitalLink("01/05414195535264/10/ABC", "5414195");
            var result = Controller.ResolveLinkType(digitalLink, new("gs1:pip"), [new("*/*")], [new("en-GB")], context);

            var redirectResult = Assert.IsInstanceOfType<RedirectResult>(result);

            Assert.IsFalse(redirectResult.Permanent);
            Assert.AreEqual("https://test.url/en", redirectResult.Url);
        }

        [TestMethod]
        public void ShouldReturnMultipleResultsIfNoBestMatchIsFound()
        {
            var context = TestDataProvider.CreateContext();
            var digitalLink = TestDataProvider.CreateDigitalLink("01/04047111050247", "4047111");
            var result = Controller.ResolveLinkType(digitalLink, new("gs1:pip"), [new("*/*")], [], context);

            var multipleChoicesResult = Assert.IsInstanceOfType<MultipleChoicesObjectResult>(result);
            var resolutionResult = Assert.IsInstanceOfType<ResolutionResult>(multipleChoicesResult.Value);

            Assert.HasCount(2, resolutionResult.Links);
        }
    }

    #endregion

    #region ResolveDefaultLink

    [TestClass]
    public sealed class ResolveDefaultLink
    {
        [TestMethod]
        public void ShouldReturnStatusCodeNotFoundWhenNoLinkIsConfigured()
        {
            var context = TestDataProvider.CreateContext();
            var digitalLink = new DigitalLink() { AIs = [new KeyValue { Key = new() { Type = AIType.PrimaryKey, Code = "01" }, Value = "undefined", Issues = [] }], CompanyPrefix = "654321", HostUrl = "https://test.com", QueryString = [] };
            var result = Controller.ResolveDefaultLink(digitalLink, [new("*/*")], [new("en-GB")], context);

            Assert.IsInstanceOfType<NotFoundObjectResult>(result);
        }

        [TestMethod]
        public void ShouldRedirectToLinksetByDefaultWhenNoDefaultLinkIsConfigured()
        {
            var context = TestDataProvider.CreateContext();
            var digitalLink = TestDataProvider.CreateDigitalLink("01/05414195535264/10/ABC", "5414195");
            var result = Controller.ResolveDefaultLink(digitalLink, [new("*/*")], [new("en-GB")], context);

            var redirectResult = Assert.IsInstanceOfType<RedirectResult>(result);
            
            Assert.IsFalse(redirectResult.Permanent);
            Assert.AreEqual("https://test.com/01/05414195535264/10/ABC?linkType=linkset", redirectResult.Url);
        }
    }

    #endregion
}
