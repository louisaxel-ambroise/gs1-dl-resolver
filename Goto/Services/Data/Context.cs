using Goto.Services.Data.Entities;
using Goto.Services;
using DigitalLinkToolkit.Conversion.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Security.Claims;
using Goto.Services.Data.Entities;

namespace Goto.Services.Data;

public class Context(DbContextOptions<Context> options, Clock clock) : DbContext(options)
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
        link.Property(l => l.LinkType);
        link.Property(l => l.MediaType);
        link.Property(l => l.ActiveFrom).IsRequired().HasConversion(new DateTimeOffsetToBinaryConverter());
        link.Property(a => a.ActiveUntil).IsRequired().HasConversion(new DateTimeOffsetToBinaryConverter());
        link.Property(l => l.Language).HasConversion(v => v.ToString(), v => new Language(v));
        link.HasQueryFilter("ActiveLinks", l => l.ActiveFrom <= clock.UtcNow && l.ActiveUntil >= clock.UtcNow);

        var insight = modelBuilder.Entity<Insight>();
        insight.HasKey(a => a.Id);
        insight.Property(a => a.RecordDate).IsRequired().HasConversion(new DateTimeOffsetToBinaryConverter());
        insight.Property(a => a.RequestDate).IsRequired().HasConversion(new DateTimeOffsetToBinaryConverter());
        insight.Property(i => i.StatusCode).IsRequired();
        insight.Property(i => i.LinkCount).IsRequired();
        insight.Property(i => i.Url).IsRequired();
    }
}
