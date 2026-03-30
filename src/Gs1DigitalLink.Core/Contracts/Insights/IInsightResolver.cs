using Gs1DigitalLink.Core.Model;
using Gs1DigitalLink.Core.Services.Insights;

namespace Gs1DigitalLink.Core.Contracts.Insights;

public interface IInsightResolver
{
    IEnumerable<Insight> ListInsights(string digitalLink, ListInsightsOptions options);
}
