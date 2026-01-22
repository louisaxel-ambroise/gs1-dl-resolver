namespace Gs1DigitalLink.Core.Model;

public interface IDomainEvent
{
    public DateTimeOffset RaisedAt { get; set; }
}
