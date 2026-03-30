using Gs1DigitalLink.Core.Contracts.Insights;
using Gs1DigitalLink.Core.Model;
using System.Threading.Channels;

namespace Gs1DigitalLink.Core.Services.Insights;

internal sealed class InsightRecorder(Channel<Insight> channel) : IInsightRecorder
{
    public void Record(Insight insight)
    {
        channel.Writer.TryWrite(insight);
    }
}