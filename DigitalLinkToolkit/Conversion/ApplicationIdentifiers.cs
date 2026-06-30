using DigitalLinkToolkit.Conversion.DTOs;
using System.Text.Json;

namespace DigitalLinkToolkit.Conversion;

public record ApplicationIdentifiers
{
    public static ApplicationIdentifiers Shared { get; private set; } = new ();

    public IReadOnlyList<AIIdentifier> Identifiers { get; init; } = [];
    public Dictionary<string, int> CodeLength { get; init; } = [];

    public bool TryGet(string key, out AIIdentifier ai)
    {
        ai = Identifiers.SingleOrDefault(x => x.Code == key || x.ShortCode == key, AIIdentifier.None);

        return ai != AIIdentifier.None;
    }

    public static void Initialize(string fileName, JsonSerializerOptions? options)
    {
        using var fileStream = File.OpenRead(fileName);

        Shared = JsonSerializer.Deserialize<ApplicationIdentifiers>(fileStream, options) ?? new();
    }
}