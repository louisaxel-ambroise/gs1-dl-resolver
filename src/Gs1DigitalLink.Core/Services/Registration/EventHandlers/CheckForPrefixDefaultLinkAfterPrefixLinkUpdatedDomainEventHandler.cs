using Gs1DigitalLink.Core.Model;
using Gs1DigitalLink.Core.Model.Events;
using Gs1DigitalLink.Core.Model.Interfaces;

namespace Gs1DigitalLink.Core.Services.Registration.EventHandlers;

public class CheckForPrefixDefaultLinkAfterPrefixLinkUpdatedDomainEventHandler : 
    IDomainEventHandler<PrefixLinkRegisteredDomainEvent>,
    IDomainEventHandler<PrefixLinksDeactivatedDomainEvent>
{
    public void Handle(PrefixLinkRegisteredDomainEvent domainEvent)
    {
        UpdatePrefix(domainEvent.Prefix, domainEvent.RaisedAt);
    }

    public void Handle(PrefixLinksDeactivatedDomainEvent domainEvent)
    {
        UpdatePrefix(domainEvent.Prefix, domainEvent.RaisedAt);
    }

    private static void UpdatePrefix(Prefix prefix, DateTimeOffset raisedAt)
    {
        var hasDefaultLink = prefix.Links.Any(l => l.IsApplicableAt(raisedAt) && l.IsDefaultLink);

        prefix.SetLinksetAsDefault(!hasDefaultLink);
    }
}
