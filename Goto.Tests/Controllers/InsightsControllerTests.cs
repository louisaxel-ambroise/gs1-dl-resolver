using Goto.Controllers;
using Goto.Controllers.Results;
using Microsoft.AspNetCore.Mvc;

namespace Goto.Tests.Controllers;

public static class InsightsControllerTests
{
    public static InsightsController Controller { get; set; } = new();

    #region ListInsights

    [TestClass]
    public sealed class ListInsights
    {
        [TestMethod]
        public void ShouldReturnInsights()
        {
            var companyPrefix = "5414195";
            var context = TestDataProvider.CreateContext();
            var principal = TestDataProvider.CreateUserPrincipal(companyPrefix);
            var result = Controller.ListInsights(context, principal);

            var okObjectResult = Assert.IsInstanceOfType<OkObjectResult>(result);
            var insightSummaryResult = Assert.IsInstanceOfType<InsightSummaryResult>(okObjectResult.Value);

            Assert.AreEqual(2, insightSummaryResult.Count);
            Assert.HasCount(insightSummaryResult.Count, insightSummaryResult.Data);
        }

        [TestMethod]
        public void ShouldFilterInsightsBasedOnCompanyPrefix()
        {
            var companyPrefix = "5414194";
            var context = TestDataProvider.CreateContext();
            var principal = TestDataProvider.CreateUserPrincipal(companyPrefix);
            var result = Controller.ListInsights(context, principal);

            var okObjectResult = Assert.IsInstanceOfType<OkObjectResult>(result);
            var insightSummaryResult = Assert.IsInstanceOfType<InsightSummaryResult>(okObjectResult.Value);

            Assert.AreEqual(0, insightSummaryResult.Count);
        }
    }

    #endregion

    #region GetInsightDetails

    [TestClass]
    public sealed class GetInsightDetails
    {
        [TestMethod]
        public void ShouldReturnDetails()
        {
            var companyPrefix = "5414195";
            var context = TestDataProvider.CreateContext();
            var principal = TestDataProvider.CreateUserPrincipal(companyPrefix);
            var result = Controller.GetInsightDetails("01/05414195535264", context, principal);

            var okObjectResult = Assert.IsInstanceOfType<OkObjectResult>(result);
            var insightDetailResult = Assert.IsInstanceOfType<InsightDetailResult>(okObjectResult.Value);

            Assert.AreEqual(2, insightDetailResult.Count);
            Assert.HasCount(insightDetailResult.Count, insightDetailResult.Data);
        }

        [TestMethod]
        public void ShouldFilterInsightsBasedOnCompanyPrefix()
        {
            var companyPrefix = "5414194";
            var context = TestDataProvider.CreateContext();
            var principal = TestDataProvider.CreateUserPrincipal(companyPrefix);
            var result = Controller.GetInsightDetails("01/05414195535264", context, principal);

            var okObjectResult = Assert.IsInstanceOfType<OkObjectResult>(result);
            var insightDetailResult = Assert.IsInstanceOfType<InsightDetailResult>(okObjectResult.Value);

            Assert.AreEqual(0, insightDetailResult.Count);
        }
    }

    #endregion
}
