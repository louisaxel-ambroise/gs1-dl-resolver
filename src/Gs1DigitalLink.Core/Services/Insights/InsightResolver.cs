using Gs1DigitalLink.Core.Model;
using Microsoft.EntityFrameworkCore;

namespace Gs1DigitalLink.Core.Services.Insights;

public interface IInsightResolver
{
    IEnumerable<Insight> ListInsights(string digitalLink, ListInsightsOptions options);
}

public class InsightResolver(ResolverContext context, TimeProvider timeProvider) : IInsightResolver
{
    public IEnumerable<Insight> ListInsights(string digitalLink, ListInsightsOptions options)
    {
        var minDate = timeProvider.GetUtcNow().Subtract(TimeSpan.FromDays(options.Days));

        return context.Insights.AsNoTracking()
            .Where(i => i.DigitalLink == digitalLink)
            .Where(i => i.Timestamp >= minDate)
            .AsEnumerable();
    }
}

public sealed record ListInsightsOptions
{
    public int Days { get; init; }
}