namespace Gs1DigitalLink.Core.Model;

/* --------------------------- */

public interface IDomainEventHandler<T> where T : IDomainEvent
{
    void Handle(T domainEvent);
}
