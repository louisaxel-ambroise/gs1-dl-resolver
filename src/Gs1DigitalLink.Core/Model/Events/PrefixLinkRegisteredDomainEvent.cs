using Gs1DigitalLink.Core.Model.Interfaces;

namespace Gs1DigitalLink.Core.Model.Events;

public record PrefixLinkRegisteredDomainEvent(Prefix Prefix, Link Link) : IDomainEvent
{
    public DateTimeOffset RaisedAt { get; set; }
}
