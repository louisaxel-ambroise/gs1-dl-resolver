using Gs1DigitalLink.Core.Model;
using System.Threading.Channels;

namespace Gs1DigitalLink.Core.Services.Insights;

public interface IInsightRecorder
{
    void Record(Insight insight);
}

public class InsightRecorder(Channel<Insight> channel) : IInsightRecorder
{
    public void Record(Insight insight)
    {
        channel.Writer.TryWrite(insight);
    }
}