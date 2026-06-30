using DigitalLinkToolkit.Translation.Model.EPCs;
using DigitalLinkToolkit.Translation.Model.Tables;
using System.Text.Json;

namespace DigitalLinkToolkit.Translation;

public sealed class TdtEngineBuilder
{
    private readonly List<Scheme> _schemes = [];
    private readonly List<Table> _tables = [];

    public TdtEngineBuilder AddDefinitionFile(string definitionFilePath) => AddDefinitionFile(File.OpenRead(definitionFilePath));

    private TdtEngineBuilder AddDefinitionFile(FileStream fileStream)
    {
        var deserialized = JsonSerializer.Deserialize<DefinitionFile>(fileStream) ?? throw new InvalidOperationException("Invalid definition file");

        if (deserialized?.TagDataTranslation?.Scheme is not null)
        {
            _schemes.Add(deserialized.TagDataTranslation.Scheme);
        }

        return this;
    }

    public TdtEngineBuilder AddTableFile(string tableFilePath) => AddTableFile(File.OpenRead(tableFilePath));

    private TdtEngineBuilder AddTableFile(FileStream fileStream)
    {
        var deserialized = JsonSerializer.Deserialize<Table>(fileStream) ?? throw new InvalidOperationException("Invalid definition file");
        _tables.Add(deserialized);

        return this;
    }

    public TdTEngine BuildEngine() => new(_schemes, _tables);
}
