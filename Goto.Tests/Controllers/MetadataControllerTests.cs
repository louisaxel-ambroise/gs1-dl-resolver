using Goto.Controllers;
using Goto.Controllers.Results;
using Microsoft.AspNetCore.Mvc;

namespace Goto.Tests.Controllers;

public static class MetadataControllerTests
{
    public static MetadataController Controller { get; set; } = new();

    #region GetMetadataInformation

    [TestClass]
    public sealed class GetMetadataInformation()
    {
        private static readonly string[] ExpectedSupportedPrimaryKeys = ["all"];

        [TestMethod]
        public void ShouldReturnExpectedMetadata()
        {
            var result = Controller.GetMetadataInformation(new Uri("https://test.url/.well-known/gs1resolver"));

            var okObjectResult = Assert.IsInstanceOfType<OkObjectResult>(result);
            var metadata = Assert.IsInstanceOfType<MetadataResult>(okObjectResult.Value);

            Assert.AreEqual("https://test.url", metadata.ResolverRoot);
            Assert.AreEqual("GOTO v0.1", metadata.Name);
            Assert.IsTrue(metadata.LinkTypeDefaultCanBeLinkset);
            Assert.AreEqual("GOTO", metadata.Contact.Fn);
            CollectionAssert.AreEquivalent(ExpectedSupportedPrimaryKeys, metadata.SupportedPrimaryKeys);
        }
    }

    #endregion
}
