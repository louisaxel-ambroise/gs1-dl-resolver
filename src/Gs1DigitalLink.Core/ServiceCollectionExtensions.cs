using DecoratR;
using Gs1DigitalLink.Core.Contracts.Conversion;
using Gs1DigitalLink.Core.Contracts.Insights;
using Gs1DigitalLink.Core.Contracts.Registration;
using Gs1DigitalLink.Core.Contracts.Resolution;
using Gs1DigitalLink.Core.Model;
using Gs1DigitalLink.Core.Model.Interfaces;
using Gs1DigitalLink.Core.Services.Conversion;
using Gs1DigitalLink.Core.Services.Conversion.Utils;
using Gs1DigitalLink.Core.Services.Conversion.Utils.Validation;
using Gs1DigitalLink.Core.Services.Insights;
using Gs1DigitalLink.Core.Services.Registration;
using Gs1DigitalLink.Core.Services.Resolution;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Threading.Channels;

namespace Gs1DigitalLink.Core;

public static class ServiceCollectionExtensions
{
    public static void AddDigitalLinkCore(this IServiceCollection services)
    {
        var jsonOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

        using (var file = File.OpenRead("Documents/gcpprefixformatlist.xml"))
        {
            CompanyPrefix.Initialize(file);
        }
        using (var file = File.OpenRead("Documents/ApplicationIdentifiers.json"))
        {
            services.AddSingleton(JsonSerializer.Deserialize<ApplicationIdentifiers>(file, jsonOptions) ?? new() { Identifiers = [], CodeLength = [] });
        }
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(Channel.CreateUnbounded<Insight>());
        services.AddSingleton<IInsightRecorder, InsightRecorder>();
        services.AddSingleton<IDigitalLinkConverter, DigitalLinkConverter>();
        services.AddSingleton<IDigitalLinkPrefixConverter, DigitalLinkPrefixConverter>();
        services.AddScoped<ILinkRegistrator, LinkRegistrator>();
        services.Decorate<IDigitalLinkResolver>()
            .With<InsightDigitalLinkResolver>()
            .Then<DigitalLinkResolver>()
            .AsScoped()
            .Apply();

        services.AddDbContext<ResolverContext>();
        services.AddScoped<IInsightResolver, InsightResolver>();

        typeof(Aggregate).Assembly.GetTypes().Where(t => t.IsClass && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))).ToList()
            .ForEach(entityType => services.AddScoped(entityType.GetInterfaces()[0], entityType));
    }
}

