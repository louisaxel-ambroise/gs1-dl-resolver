namespace Gs1DigitalLink.Core.Model;

public sealed class Link : Entity<Prefix>
{
    public required Language? Language { get; set; }
    public required string Title { get; set; }
    public required string RedirectUrl { get; set; }
    public required string LinkType { get; set; }
    public required DateRange Availability { get; set; }

    public bool IsDefaultLink() => LinkType.StartsWith("gs1:defaultLink");

    public bool Equals(Link other)
    {
        return Equals(other.Language, Language) && other.LinkType.Equals(LinkType);
    }

    public bool Overlapse(Link other)
    {
        return Availability.Overlapse(other.Availability);
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
