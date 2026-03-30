using Gs1DigitalLink.Core.Contracts;
using Gs1DigitalLink.Core.Model;
using Gs1DigitalLink.Core.Model.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Gs1DigitalLink.Core;

public class ResolverContext : DbContext
{
    public DbSet<Prefix> Prefixes { get; private set; }
    public DbSet<Insight> Insights { get; private set; }

    private readonly IEventDispatcher _eventDispatcher;
    private readonly TimeProvider _timeProvider;

    public ResolverContext(IEventDispatcher eventDispatcher, TimeProvider timeProvider)
    {
        _eventDispatcher = eventDispatcher;
        _timeProvider = timeProvider;
        SavingChanges += DispatchDomainEvents;
    }

    private void DispatchDomainEvents(object? sender, SavingChangesEventArgs e)
    {
        var depth = 0;
        var entries = default(IEnumerable<EntityEntry>);

        do
        {
            entries = ChangeTracker.Entries().Where(e => e.Entity is Aggregate aggregate && aggregate.PendingEvents.Any());

            foreach (var entity in entries.Select(entry => (Aggregate)entry.Entity))
            {
                DispatchDomainEvents(entity);

                entity.ClearPendingEvents();
            }

            if(depth++ > MAX_RECURSION)
            {
                throw new InvalidOperationException("Reached max recusion for domain events dispatching. Please review architecture.");
            }

        } while (entries.Any());
    }

    private void DispatchDomainEvents(Aggregate entity)
    {
        foreach (var pendingEvent in entity.PendingEvents)
        {
            pendingEvent.RaisedAt = _timeProvider.GetUtcNow();
            _eventDispatcher.Dispatch(pendingEvent);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var prefix = modelBuilder.Entity<Prefix>().ToTable(nameof(Prefix));
        prefix.HasKey(e => e.Id);
        prefix.Property(e => e.Id).UseAutoincrement();
        prefix.HasMany(e => e.Links).WithOne().HasForeignKey("PrefixId");
        prefix.Navigation(e => e.Links).AutoInclude();

        var link = modelBuilder.Entity<Link>().ToTable(nameof(Link));
        link.HasKey(e => e.Id);
        link.Property(e => e.Id).UseAutoincrement();
        link.ComplexProperty(l => l.Availability, cfg =>
        {
            cfg.Property(a => a.From).HasConversion(dto => dto.ToUnixTimeSeconds(), s => DateTimeOffset.FromUnixTimeSeconds(s)).HasColumnName("AvailableFrom");
            cfg.Property(a => a.To).HasConversion(dto => dto == null ? default(long?) : dto.Value.ToUnixTimeSeconds(), s => s == null ? default(DateTimeOffset?) : DateTimeOffset.FromUnixTimeSeconds(s.Value)).HasColumnName("AvailableTo");
        });
        link.Property(e => e.Language).HasConversion(lang => lang == null ? string.Empty : lang.ToString(), s => s);

        var insight = modelBuilder.Entity<Insight>().ToTable(nameof(Insight));
        insight.HasKey(e => e.Id);
        insight.Property(e => e.Id).UseAutoincrement();
        insight.Property(e => e.Timestamp).HasConversion(dto => dto.ToUnixTimeSeconds(), s => DateTimeOffset.FromUnixTimeSeconds(s));
        insight.Property(e => e.Languages).HasConversion(lang => string.Join(',', lang), s => s.Split(','));
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source=registry.db");

    const int MAX_RECURSION = 5;
}

