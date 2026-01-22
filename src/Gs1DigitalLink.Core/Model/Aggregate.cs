namespace Gs1DigitalLink.Core.Model;

public abstract class Aggregate
{
    public int Id { get; internal set; }

    public void Raise<T>(T domainEvent) where T : IDomainEvent
    {
        _pendingEvents.Add(domainEvent);
    }

    internal IReadOnlyCollection<IDomainEvent> PendingEvents => _pendingEvents.AsReadOnly();
    internal void ClearPendingEvents() => _pendingEvents.Clear();

    private readonly IList<IDomainEvent> _pendingEvents = [];
}

public abstract class Entity<T> where T : Aggregate
{
    public int Id { get; internal set; }
}