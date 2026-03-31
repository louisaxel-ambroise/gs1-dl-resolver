using Gs1DigitalLink.Core.Model;

namespace Gs1DigitalLink.Core.Contracts.Insights;

public interface IInsightRecorder
{
    void Record(Insight insight);
}