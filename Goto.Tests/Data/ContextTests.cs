namespace Goto.Tests.Data;

[TestClass]
public sealed class ContextTests
{
    [TestMethod]
    public void AnchorsForUserShouldFilterTheAnchorsForLoggedUser()
    {
        var companyPrefix = "5414195";
        var context = TestDataProvider.CreateContext();
        var loggedUser = TestDataProvider.CreateUserPrincipal(companyPrefix);

        var anchors = context.AnchorsForUser(loggedUser);

        Assert.IsGreaterThan(0, anchors.Count());
        Assert.IsTrue(anchors.All(a => a.CompanyPrefix == companyPrefix));
    }

    [TestMethod]
    public void InsightsForUserShouldFilterTheInsightsForLoggedUser()
    {
        var companyPrefix = "5414195";
        var context = TestDataProvider.CreateContext();
        var loggedUser = TestDataProvider.CreateUserPrincipal(companyPrefix);

        var insights = context.InsightsForUser(loggedUser);

        Assert.IsGreaterThan(0, insights.Count());
        Assert.IsTrue(insights.All(a => a.CompanyPrefix == companyPrefix));
    }
}
