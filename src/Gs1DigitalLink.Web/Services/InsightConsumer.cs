using Gs1DigitalLink.Core;
using Gs1DigitalLink.Core.Model;
using System.Threading.Channels;

namespace Gs1DigitalLink.Web.Services;

internal sealed class InsightConsumer(Channel<Insight> channel, IServiceProvider serviceProvider, ILogger<InsightConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var insight in channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                using var context = scope.ServiceProvider.GetRequiredService<ResolverContext>();

                context.Insights.Add(insight);
                context.SaveChanges();
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Unable to process Insight channel message");
            }
        }
    }
}