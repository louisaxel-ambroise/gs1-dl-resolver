namespace Gs1DigitalLink.Core.Model;

public record PrefixLinkRegisteredDomainEvent(Prefix Prefix, Link Link) : IDomainEvent
{
    public DateTimeOffset RaisedAt { get; set; }
}
