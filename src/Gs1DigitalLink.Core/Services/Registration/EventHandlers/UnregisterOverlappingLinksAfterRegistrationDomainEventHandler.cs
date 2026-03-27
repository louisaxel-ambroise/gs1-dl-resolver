using Gs1DigitalLink.Core.Model.Events;
using Gs1DigitalLink.Core.Model.Interfaces;

namespace Gs1DigitalLink.Core.Services.Registration.EventHandlers;

public sealed class UnregisterOverlappingLinksAfterRegistrationDomainEventHandler : IDomainEventHandler<PrefixLinkRegisteredDomainEvent>
{
    public void Handle(PrefixLinkRegisteredDomainEvent domainEvent)
    {
        var link = domainEvent.Link;
        var links = domainEvent.Prefix.Links.Except([link]);

        foreach (var overlappingLink in links.Where(l => l.IsEquivalentTo(link) && l.Availability.Overlapse(link.Availability)))
        {
            overlappingLink.EndAvailability(link.Availability.From);
        }
    }
}