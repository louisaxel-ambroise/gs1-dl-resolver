namespace Gs1DigitalLink.Core.Model;

public sealed class DateRange : ValueObject
{
    public DateRange(DateTimeOffset from) : this(from, null)
    {
    }

    public DateRange(DateTimeOffset from, DateTimeOffset? to)
    {
        From = from;
        To = to;
    }

    public DateTimeOffset From { get; private set; }
    public DateTimeOffset? To { get; private set; }

    internal bool Includes(DateTimeOffset applicability)
    {
        return applicability >= From && (To is null || applicability <= To);
    }

    internal bool Overlapse(DateRange other)
    {
        return From >= other.From && (other.To is null || From <= other.To)
            || From <= other.From && (To is null || To >= other.From);
    }

    public void SetEndDate(DateTimeOffset dateTimeOffset)
    {
        To = dateTimeOffset;
    }
}
