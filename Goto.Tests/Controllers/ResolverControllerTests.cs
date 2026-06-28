using Goto.Controllers;
using Goto.Controllers.Results;
using Goto.Services.Conversion;
using Goto.Services.Conversion.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Goto.Tests.Controllers;

public static class ResolverControllerTests
{
    public static ResolverController Controller { get; set; } = new();

    #region ResolveLinkset

    [TestClass]
    public sealed class ResolveLinkset()
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
}
