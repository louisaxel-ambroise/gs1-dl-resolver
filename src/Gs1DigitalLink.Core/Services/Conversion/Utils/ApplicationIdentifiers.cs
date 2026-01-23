namespace Gs1DigitalLink.Core.Services.Conversion.Utils;

internal record ApplicationIdentifiers
{
    public required IReadOnlyList<Identifier> Identifiers { get; init; }
    public required Dictionary<string, int> CodeLength { get; init; }

    public bool TryGet(string key, out Identifier ai)
    {
        ai = Identifiers.SingleOrDefault(x => x.Code == key || x.ShortCode == key, Identifier.None);

        return ai != Identifier.None;
    }
}