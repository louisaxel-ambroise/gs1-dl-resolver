using Gs1DigitalLink.Core.Model;
using Microsoft.EntityFrameworkCore;
using System;

namespace Gs1DigitalLink.Core;

public class DigitalLinkContext : DbContext
{
    public DbSet<Prefix> Prefixes { get; private set; }
    public DbSet<Insight> Insights { get; private set; }

    private readonly IEventDispatcher _eventDispatcher;
    private readonly TimeProvider _timeProvider;

    public DigitalLinkContext(IEventDispatcher eventDispatcher, TimeProvider timeProvider)
    {
        _eventDispatcher = eventDispatcher;
        _timeProvider = timeProvider;
        SavingChanges += DispatchDomainEvents;
    }

    private void DispatchDomainEvents(object? sender, SavingChangesEventArgs e)
    {
        ChangeTracker.Entries().Where(e => e.Entity is Aggregate entity).ToList()
            .ForEach(entry =>
            {
                var entity = (Aggregate)entry.Entity;
                var events = entity.PendingEvents.ToList();
                events.ForEach(e =>
                {
                    e.RaisedAt = _timeProvider.GetUtcNow();
                    _eventDispatcher.Dispatch(e);
                });
                entity.ClearPendingEvents();
            });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var prefix = modelBuilder.Entity<Prefix>().ToTable("Prefix");
        prefix.HasKey(e => e.Id);
        prefix.Property(e => e.Id).UseAutoincrement();
        prefix.HasMany(e => e.Links).WithOne().HasForeignKey("PrefixId");
        prefix.Navigation(e => e.Links).AutoInclude();

        var link = modelBuilder.Entity<Link>().ToTable("Link");
        link.HasKey(e => e.Id);
        link.Property(e => e.Id).UseAutoincrement();
        link.ComplexProperty(l => l.Availability, cfg =>
        {
            cfg.Property(a => a.From).HasConversion(dto => dto.ToUnixTimeSeconds(), s => DateTimeOffset.FromUnixTimeSeconds(s)).HasColumnName("AvailableFrom");
            cfg.Property(a => a.To).HasConversion(dto => dto == null ? default(long?) : dto.Value.ToUnixTimeSeconds(), s => s == null ? default(DateTimeOffset?) : DateTimeOffset.FromUnixTimeSeconds(s.Value)).HasColumnName("AvailableTo");
        });
        link.Property(e => e.Language).HasConversion(lang => lang.ToString(), s => s);

        var insight = modelBuilder.Entity<Insight>().ToTable("Insight");
        insight.HasKey(e => e.Id);
        insight.Property(e => e.Id).UseAutoincrement();
        insight.Property(e => e.Timestamp).HasConversion(dto => dto.ToUnixTimeSeconds(), s => DateTimeOffset.FromUnixTimeSeconds(s));
        insight.Property(e => e.Languages).HasConversion(lang => string.Join(',', lang), s => s.Split(','));
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source=registry.db");
}

