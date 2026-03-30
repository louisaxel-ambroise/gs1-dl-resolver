using Gs1DigitalLink.Core.Contracts.Insights;
using Gs1DigitalLink.Core.Contracts.Resolution;
using Gs1DigitalLink.Core.Model;
using Gs1DigitalLink.Core.Services.Conversion;
using Gs1DigitalLink.Core.Services.Resolution;

namespace Gs1DigitalLink.Core.Services.Insights;

internal sealed class InsightDigitalLinkResolver(IDigitalLinkResolver resolver, ILanguageContext languageContext, IInsightRecorder insightRecorder, TimeProvider timeProvider) : IDigitalLinkResolver
{
    public IResolutionResult ResolveLinkSet(DigitalLink digitalLink, DateTimeOffset applicability)
    {
        return resolver.ResolveLinkSet(digitalLink, applicability);
    }

    public IResolutionResult ResolveLinkType(DigitalLink digitalLink, DateTimeOffset applicability, string? linkType)
    {
        var result = resolver.ResolveLinkType(digitalLink, applicability, linkType);

        var insight = new Insight
        {
            DigitalLink = digitalLink.ToShortString(),
            Timestamp = timeProvider.GetUtcNow(),
            LinkType = linkType,
            Languages = languageContext.GetLanguages().Select(language => language.ToString()),
            CandidateCount = result.Links.Count()
        };

        insightRecorder.Record(insight);

        return result;
    }
}
