namespace Goto.Services.Data.Entities;

public record LinkType(string Value)
{
    public static readonly string[] AllValues =
    [
        "gs1:activityIdeas",
        "gs1:allergenInfo",
        "gs1:appDownload",
        "gs1:backgroundInfo",
        "gs1:brandHomepageClinical",
        "gs1:brandHomepagePatient",
        "gs1:careersInfo",
        "gs1:certificationInfo",
        "gs1:consumerHandlingStorageInfo",
        "gs1:dpp",
        "gs1:eifu",
        "gs1:epcis",
        "gs1:epil",
        "gs1:eventsInfo",
        "gs1:faqs",
        "gs1:handledBy",
        "gs1:hasRetailers",
        "gs1:homepage",
        "gs1:ingredientsInfo",
        "gs1:instructions",
        "gs1:jws",
        "gs1:leaveReview",
        "gs1:locationInfo",
        "gs1:logisticsInfo",
        "gs1:loyaltyProgram",
        "gs1:masterData",
        "gs1:menuInfo",
        "gs1:nutritionalInfo",
        "gs1:openingHoursInfo",
        "gs1:paymentLink",
        "gs1:pip",
        "gs1:productSustainabilityInfo",
        "gs1:promotion",
        "gs1:purchaseSuppliesOrAccessories",
        "gs1:quickStartGuide",
        "gs1:recallStatus",
        "gs1:recipeInfo",
        "gs1:registerProduct",
        "gs1:registryEntry",
        "gs1:relatedImage",
        "gs1:relatedVideo",
        "gs1:reportFound",
        "gs1:review",
        "gs1:safetyInfo",
        "gs1:scheduleTime",
        "gs1:serviceInfo",
        "gs1:smartLabel",
        "gs1:smpc",
        "gs1:socialMedia",
        "gs1:statisticInfo",
        "gs1:subscribe",
        "gs1:support",
        "gs1:sustainabilityInfo",
        "gs1:traceability",
        "gs1:tutorial",
        "gs1:userAgreement",
        "gs1:verificationService",
        "gs1:whatsInTheBox"
    ];

    public static LinkType Parse(string linkType)
    {
        return AllValues.Contains(linkType)
            ? new LinkType(linkType)
            : throw new ArgumentOutOfRangeException(nameof(linkType));
    }

    public override string ToString()
    {
        return Value;
    }
}
