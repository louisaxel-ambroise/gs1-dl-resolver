using Goto.Services.Data.Entities;

namespace Goto.Tests.Data.Entities;

[TestClass]
public class LinkTypeTests
{
    [TestMethod]
    public void ToStringShouldWork()
    {
        var sut = new LinkType("gs1:pip");
        var result = sut.ToString();

        Assert.AreEqual("gs1:pip", result);
    }

    [TestMethod]
    [DataRow("gs1:activityIdeas")]
    [DataRow("gs1:allergenInfo")]
    [DataRow("gs1:appDownload")]
    [DataRow("gs1:backgroundInfo")]
    [DataRow("gs1:brandHomepageClinical")]
    [DataRow("gs1:brandHomepagePatient")]
    [DataRow("gs1:careersInfo")]
    [DataRow("gs1:certificationInfo")]
    [DataRow("gs1:consumerHandlingStorageInfo")]
    [DataRow("gs1:dpp")]
    [DataRow("gs1:eifu")]
    [DataRow("gs1:epcis")]
    [DataRow("gs1:epil")]
    [DataRow("gs1:eventsInfo")]
    [DataRow("gs1:faqs")]
    [DataRow("gs1:handledBy")]
    [DataRow("gs1:hasRetailers")]
    [DataRow("gs1:homepage")]
    [DataRow("gs1:ingredientsInfo")]
    [DataRow("gs1:instructions")]
    [DataRow("gs1:jws")]
    [DataRow("gs1:leaveReview")]
    [DataRow("gs1:locationInfo")]
    [DataRow("gs1:logisticsInfo")]
    [DataRow("gs1:loyaltyProgram")]
    [DataRow("gs1:masterData")]
    [DataRow("gs1:menuInfo")]
    [DataRow("gs1:nutritionalInfo")]
    [DataRow("gs1:openingHoursInfo")]
    [DataRow("gs1:paymentLink")]
    [DataRow("gs1:pip")]
    [DataRow("gs1:productSustainabilityInfo")]
    [DataRow("gs1:promotion")]
    [DataRow("gs1:purchaseSuppliesOrAccessories")]
    [DataRow("gs1:quickStartGuide")]
    [DataRow("gs1:recallStatus")]
    [DataRow("gs1:recipeInfo")]
    [DataRow("gs1:registerProduct")]
    [DataRow("gs1:registryEntry")]
    [DataRow("gs1:relatedImage")]
    [DataRow("gs1:relatedVideo")]
    [DataRow("gs1:reportFound")]
    [DataRow("gs1:review")]
    [DataRow("gs1:safetyInfo")]
    [DataRow("gs1:scheduleTime")]
    [DataRow("gs1:serviceInfo")]
    [DataRow("gs1:smartLabel")]
    [DataRow("gs1:smpc")]
    [DataRow("gs1:socialMedia")]
    [DataRow("gs1:statisticInfo")]
    [DataRow("gs1:subscribe")]
    [DataRow("gs1:support")]
    [DataRow("gs1:sustainabilityInfo")]
    [DataRow("gs1:traceability")]
    [DataRow("gs1:tutorial")]
    [DataRow("gs1:userAgreement")]
    [DataRow("gs1:verificationService")]
    [DataRow("gs1:whatsInTheBox")]
    public void ShouldParseValidValues(string linkType)
    {
        var result = LinkType.Parse(linkType);

        Assert.IsNotNull(result);
        Assert.AreEqual(linkType, result.Value);
    }
    [TestMethod]
    public void ShouldNotParseInvalidValues()
    {
        var act = () => LinkType.Parse("gs1:nosuchelt");

        var ex = Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.AreEqual("linkType", ex.ParamName);
    }
}
