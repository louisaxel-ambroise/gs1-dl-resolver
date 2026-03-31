using Gs1DigitalLink.Core.Model.Interfaces;

namespace Gs1DigitalLink.Core.Contracts;

public interface IEventDispatcher
{
    void Dispatch(IDomainEvent domainEvent);
}
