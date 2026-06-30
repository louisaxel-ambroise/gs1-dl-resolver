using System.Text.Json;
using System.Text.Json.Serialization;

namespace DigitalLinkToolkit.Translation.Model.Tables;

public class Table
{
    [JsonPropertyName("tableID")]
    public required string TableId { get; set; }
    [JsonPropertyName("date")]
    public required string Date { get; set; }
    [JsonPropertyName("description")]
    public required string Description { get; set; }
    [JsonPropertyName("columns")]
    public IEnumerable<ColumnDefinition> Columns { get; set; } = [];
    [JsonPropertyName("rows")]
    public IEnumerable<Row> Rows { get; set; } = [];
}

public class Row
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement> RowData { get; set; } = [];

    public int GetNumber(string columnName)
    {
        if(RowData.TryGetValue(columnName, out var jsonValue))
        {
            return int.Parse(jsonValue.GetString() ?? "0");
        }

        return 0;
    }

    public string GetString(string columnName)
    {
        if (RowData.TryGetValue(columnName, out var jsonValue))
        {
            return jsonValue.GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    public Row? ToSecondComponentRow()
    {
        if (RowData.ContainsKey("j"))
        {
            return new Row
            {
                RowData = new()
                {
                    { "c", RowData["j"] },
                    { "d", RowData.TryGetValue("k", out var k) ? k : default },
                    { "e", RowData.TryGetValue("l", out var l) ? l : default },
                    { "f", RowData.TryGetValue("m", out var m) ? m : default },
                    { "g", RowData.TryGetValue("n", out var n) ? n : default },
                    { "h", RowData.TryGetValue("o", out var o) ? o : default },
                }
            };
        }

        return null;
    }
}

public class ColumnDefinition
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("description")]
    public required string Description { get; set; }
    [JsonPropertyName("encodingIndicator")]
    public int? EncodingIndicator { get; set; }
    [JsonPropertyName("specSection")]
    public string? SpecSection { get; set; }
}