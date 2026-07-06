using Goto.Services.Data.Enums;

namespace Goto.Services.Data.Entities;

public sealed class AnchorLink
{
    public int Id { get; set; }
    public int AnchorId { get; set; }
    public required string Title { get; set; }
    public required string RedirectUrl { get; set; }
    public required string LinkType { get; set; }
    public required Language Language { get; set; }
    public string? MediaType { get; set; }
    public required DateTimeOffset ActiveFrom { get; set; }
    public required DateTimeOffset ActiveUntil { get; set; }
    public bool IsDefault { get; set; }

    public Match MatchesLanguage(Language language)
    {
        return Language?.Matches(language) ?? Match.NoMatch;
    }

    public Match MatchesMediaType(string mediaType)
    {
        return MediaType == mediaType
            ? Match.FullMatch
            : Match.NoMatch;
    }

    public void SetUnavailabilityDate(DateTimeOffset activeUntil)
    {
        if(ActiveUntil < activeUntil)
        {
            throw new InvalidOperationException("Cannot deactivate a link already inactive");
        }

        ActiveUntil = activeUntil;
    }
}
