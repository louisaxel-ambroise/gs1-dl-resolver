namespace Gs1DigitalLink.Core.Model.Interfaces;

public interface IDomainEventHandler<T> where T : IDomainEvent
{
    void Handle(T domainEvent);
}
