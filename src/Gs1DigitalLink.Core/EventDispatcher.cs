using Gs1DigitalLink.Core.Model.Interfaces;

namespace Gs1DigitalLink.Core;

public interface IEventDispatcher
{
    void Dispatch(IDomainEvent domainEvent);
}
