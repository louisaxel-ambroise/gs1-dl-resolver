namespace Gs1DigitalLink.Core.Model;

public record PrefixLinksDeactivatedDomainEvent(Prefix Prefix, Language? Language, IEnumerable<Link> Links) : IDomainEvent
{
    public DateTimeOffset RaisedAt { get; set; }
}