using Gs1DigitalLink.Core.Model.Interfaces;

namespace Gs1DigitalLink.Core.Model;

public sealed class Language : ValueObject
{
    public required string Key { get; set; }
    public string? Region { get; set; }

    public override string ToString()
    {
        return string.IsNullOrEmpty(Region)
            ? Key
            : $"{Key}-{Region}";
    }

    public override bool Equals(object? obj)
    {
        return obj is Language other && other.Key == Key && other.Region == Region;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Key, Region);
    }

    public static implicit operator Language?(string? representation)
    {
        if (string.IsNullOrEmpty(representation))
        {
            return null;
        }

        var parts = representation.Split('-');

        return new() { Key = parts[0], Region = parts.Length > 1 ? parts[1] : null };
    }
}
