using Gs1DigitalLink.Core.Model;
using Gs1DigitalLink.Core.Model.Interfaces;

namespace Gs1DigitalLink.Core.Services.Registration;

public class CheckForPrefixDefaultLinkAfterRegistrationDomainEventHandler : IDomainEventHandler<PrefixLinkRegisteredDomainEvent>
{
    public void Handle(PrefixLinkRegisteredDomainEvent domainEvent)
    {
        var hasDefaultLink = domainEvent.Prefix.Links.Any(l => l.IsApplicableAt(domainEvent.RaisedAt) && l.IsDefaultLink());

        domainEvent.Prefix.SetLinksetAsDefaultLink(!hasDefaultLink);
    }
}

public class CheckForPrefixDefaultLinkAfterUnregistrationDomainEventHandler : IDomainEventHandler<PrefixLinksDeactivatedDomainEvent>
{
    public void Handle(PrefixLinksDeactivatedDomainEvent domainEvent)
    {
        var hasDefaultLink = domainEvent.Prefix.Links.Any(l => l.IsApplicableAt(domainEvent.RaisedAt) && l.IsDefaultLink());

        domainEvent.Prefix.SetLinksetAsDefaultLink(!hasDefaultLink);
    }
}