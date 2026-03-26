namespace Gs1DigitalLink.Web.Formatters.Html.Models;

public record Link
{
    public required string Href { get; init; }
    public required string Title { get; init; }
    public required IEnumerable<string> Hreflang { get; init; } = [];
    public required IEnumerable<string> LinkTypes { get; init; } = [];
}
