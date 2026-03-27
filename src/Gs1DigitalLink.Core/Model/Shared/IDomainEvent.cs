namespace Gs1DigitalLink.Core.Model.Interfaces;

public interface IDomainEvent
{
    public DateTimeOffset RaisedAt { get; set; }
}
