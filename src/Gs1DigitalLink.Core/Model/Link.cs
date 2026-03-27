using Gs1DigitalLink.Core.Model.Interfaces;

namespace Gs1DigitalLink.Core.Model;

public sealed class Link : Entity<Prefix>
{
    public required Language? Language { get; set; }
    public required string Title { get; set; }
    public required string RedirectUrl { get; set; }
    public required string LinkType { get; set; }
    public required DateRange Availability { get; set; }

    public bool IsDefaultLink => LinkType is "gs1:defaultLink" or "gs1:defaultLinkMulti";

    public bool IsEquivalentTo(Link other)
    {
        return Equals(other.Language, Language) && other.LinkType.Equals(LinkType);
    }

    internal bool IsApplicableAt(DateTimeOffset applicability)
    {
        return Availability.Includes(applicability);
    }

    internal void EndAvailability(DateTimeOffset dateTimeOffset)
    {
        Availability.SetEndDate(dateTimeOffset);
    }
}
