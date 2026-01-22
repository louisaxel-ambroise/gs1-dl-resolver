using Gs1DigitalLink.Core.Model;

namespace Gs1DigitalLink.Core;

public interface IEventDispatcher
{
    void Dispatch(IDomainEvent domainEvent);
}
