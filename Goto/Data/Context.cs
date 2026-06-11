using Goto.Data.Entities;
using Goto.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Goto.Data;

public sealed class Context : DbContext
{
    public Context(DbContextOptions<Context> options, ApiTimeProvider timeProvider) : base(options)
    {
        TimeProvider = timeProvider;
        Database.EnsureCreatedAsync();
    }

    public DbSet<Anchor> Anchors => Set<Anchor>();
    public DbSet<Insight> Insights => Set<Insight>();

    public ApiTimeProvider TimeProvider { get; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source=registry.db");
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
        link.HasQueryFilter("ActiveLinks", l => l.ActiveFrom <= TimeProvider.UtcNow && l.ActiveUntil >= TimeProvider.UtcNow);

        var insight = modelBuilder.Entity<Insight>();
        insight.HasKey(a => a.Id);
        insight.Property(a => a.RecordDate).IsRequired().HasConversion(new DateTimeOffsetToBinaryConverter());
        insight.Property(a => a.RequestDate).IsRequired().HasConversion(new DateTimeOffsetToBinaryConverter());
        insight.Property(i => i.StatusCode).IsRequired();
        insight.Property(i => i.LinkCount).IsRequired();
        insight.Property(i => i.Url).IsRequired();
    }
}
