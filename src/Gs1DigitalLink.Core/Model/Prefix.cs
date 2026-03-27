using Gs1DigitalLink.Core.Model.Events;
using Gs1DigitalLink.Core.Model.Interfaces;

namespace Gs1DigitalLink.Core.Model;

public sealed class Prefix : Aggregate
{
    public Prefix(string companyPrefix, string value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(companyPrefix.Length, 1, nameof(CompanyPrefix));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(companyPrefix.Length, 13, nameof(CompanyPrefix));

        CompanyPrefix = companyPrefix;
        Value = value;
    }

    public string CompanyPrefix { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public bool IsLinksetDefault { get; private set; }
    public IList<Link> Links { get; init; } = [];

    public void AddLink(Link link)
    {
        Links.Add(link);

        Raise(new PrefixLinkRegisteredDomainEvent(this, link));
    }

    public void SetLinksetAsDefault(bool value)
    {
        IsLinksetDefault = value;
    }

    internal void DeactivateLinks(Language? language, IEnumerable<string> linkTypes, DateTimeOffset now)
    {
        var matchingLinks = Links.Where(l => l.IsApplicableAt(now) && Equals(l.Language, language) && linkTypes.Contains(l.LinkType));

        foreach (var link in matchingLinks)
        {
            link.EndAvailability(now);
        }

        Raise(new PrefixLinksDeactivatedDomainEvent(this, language, matchingLinks));
    }
}
