using Goto.Data;
using Goto.Data.Entities;
using System.Threading.Channels;

namespace Goto.Services;

internal sealed class InsightConsumerService(Channel<Insight> channel, IServiceProvider serviceProvider, ILogger<InsightConsumerService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var insight in channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                using var context = scope.ServiceProvider.GetRequiredService<Context>();

                context.Add(insight);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to process Insight channel message");
            }
        }
    }
}