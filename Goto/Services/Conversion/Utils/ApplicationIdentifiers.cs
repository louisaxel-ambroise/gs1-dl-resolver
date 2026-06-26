using System.Text.Json;

namespace Goto.Services.Conversion.Utils;

public record ApplicationIdentifiers
{
    public static ApplicationIdentifiers Shared { get; private set; } = new ();

    public IReadOnlyList<Identifier> Identifiers { get; init; } = [];
    public Dictionary<string, int> CodeLength { get; init; } = [];

    public bool TryGet(string key, out Identifier ai)
    {
        ai = Identifiers.SingleOrDefault(x => x.Code == key || x.ShortCode == key, Identifier.None);

        return ai != Identifier.None;
    }

    public static void Initialize(string fileName, JsonSerializerOptions? options)
    {
        using var fileStream = File.OpenRead(fileName);

        Shared = JsonSerializer.Deserialize<ApplicationIdentifiers>(fileStream, options) ?? new();
    }
}