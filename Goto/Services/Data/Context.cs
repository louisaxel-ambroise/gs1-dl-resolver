using DigitalLinkToolkit.Conversion.Model;
using Goto.Infrastructure.Authentication;
using Goto.Services.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Goto.Services.Data;

public sealed class Context(DbContextOptions<Context> options, Clock clock) : DbContext(options)
{
    public IQueryable<Insight> InsightsForUser(ClaimsPrincipal user)
    {
        return Set<Insight>()
            .Where(i => i.CompanyPrefix == null || i.CompanyPrefix == user.GetCompanyPrefix());
    }

    public IQueryable<Anchor> AnchorsForUser(ClaimsPrincipal user)
    {
        return Set<Anchor>()
            .IgnoreQueryFilters(["ActiveLinks"])
            .Where(a => a.CompanyPrefix == user.GetCompanyPrefix());
    }

    public IQueryable<Anchor> AnchorsForLink(DigitalLink digitalLink)
    {
        return Set<Anchor>()
            .Where(a => digitalLink.CompanyPrefix == a.CompanyPrefix && digitalLink.GetPrefixValues().Contains(a.Prefix))
            .Where(a => a.Links.Any())
            .OrderByDescending(a => a.Prefix.Length);
    }

    public ApiKey? GetApiKeyDetails(string apiKey)
    {
        return Set<ApiKey>()
            .SingleOrDefault(k => k.Id == apiKey);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var anchor = modelBuilder.Entity<Anchor>();
        anchor.HasKey(a => a.Id);
        anchor.Property(a => a.Prefix).HasMaxLength(255).IsRequired();
        anchor.Property(a => a.Description).IsRequired();
        anchor.HasIndex(a => new { a.CompanyPrefix, a.Prefix }).IsUnique();
        anchor.HasMany(a => a.Links).WithOne().HasForeignKey(l => l.AnchorId);
        anchor.Navigation(a => a.Links).AutoInclude();

        var link = modelBuilder.Entity<AnchorLink>();
        link.HasKey(l => l.Id);
        link.Property(l => l.AnchorId);
        link.Property(l => l.RedirectUrl).HasMaxLength(4096).IsRequired();
        link.Property(l => l.Title).HasMaxLength(4096).IsRequired();
        link.Property(l => l.IsDefault);
        link.Property(l => l.ActiveFrom).IsRequired().HasConversion(new DateTimeOffsetToBinaryConverter());
        link.Property(a => a.ActiveUntil).IsRequired().HasConversion(new DateTimeOffsetToBinaryConverter());
        link.Property(l => l.LinkType).HasConversion(t => t.Value, v => new LinkType(v));
        link.Property(l => l.Language).HasConversion(v => v.ToString(), v => new Language(v));
        link.Property(l => l.MediaType).HasConversion(v => v.ToString(), v => new MediaType(v));
        link.HasQueryFilter("ActiveLinks", l => l.ActiveFrom <= clock.UtcNow && l.ActiveUntil >= clock.UtcNow);

        var insight = modelBuilder.Entity<Insight>();
        insight.HasKey(a => a.Id);
        insight.Property(a => a.RecordDate).IsRequired().HasConversion(new DateTimeOffsetToBinaryConverter());
        insight.Property(a => a.RequestDate).IsRequired().HasConversion(new DateTimeOffsetToBinaryConverter());
        insight.Property(i => i.StatusCode).IsRequired();
        insight.Property(i => i.LinkCount).IsRequired();
        insight.Property(i => i.Url).IsRequired();

        var apiKey = modelBuilder.Entity<ApiKey>();
        apiKey.HasKey(k => k.Id);
        apiKey.Property(k => k.Id).IsRequired().HasMaxLength(50);
        apiKey.Property(k => k.Name).IsRequired();
        apiKey.Property(k => k.CompanyPrefix).IsRequired();
        apiKey.Property(k => k.BeginValidityDate).IsRequired().HasConversion(new DateTimeOffsetToBinaryConverter());
        apiKey.Property(k => k.EndValidityDate).IsRequired().HasConversion(new DateTimeOffsetToBinaryConverter());
    }

    public IEnumerable<string> SeedApiKeys(ApiKeyDefinition apiKeyDefinition)
    {
        var executionTime = TimeProvider.System.GetUtcNow();
        var allKeys = Set<ApiKey>().AsTracking().Where(k => k.EndValidityDate >= executionTime).ToList();
        var processedKeys = new List<ApiKey>();

        foreach (var option in apiKeyDefinition.Keys)
        {
            var existingKey = allKeys.SingleOrDefault(k => k.Name == option.Name && k.CompanyPrefix == option.CompanyPrefix);

            if (existingKey is null)
            {
                var secret = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16));
                var hash = SHA256.HashData(Encoding.UTF8.GetBytes(secret));

                var apiKey = new ApiKey
                {
                    Id = Convert.ToBase64String(hash),
                    Name = option.Name,
                    CompanyPrefix = option.CompanyPrefix,
                    BeginValidityDate = TimeProvider.System.GetUtcNow(),
                    EndValidityDate = DateTime.MaxValue
                };

                yield return $"Created API Key '{option.Name}': '{secret}'";

                Add(apiKey);
            }
            else
            {
                processedKeys.Add(existingKey);
            }
        }

        foreach (var key in allKeys.Except(processedKeys))
        {
            key.Disable(TimeProvider.System.GetUtcNow());
            yield return $"Disabled API Key '{key.Name}'";
        }

        SaveChanges();
    }
}
