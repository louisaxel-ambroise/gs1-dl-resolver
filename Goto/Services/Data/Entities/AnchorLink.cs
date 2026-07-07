using Goto.Services.Data.Enums;

namespace Goto.Services.Data.Entities;

public sealed class AnchorLink
{
    public int Id { get; set; }
    public int AnchorId { get; set; }
    public required string Title { get; set; }
    public required string RedirectUrl { get; set; }
    public required LinkType LinkType { get; set; }
    public required Language Language { get; set; }
    public required MediaType MediaType { get; set; }
    public required DateTimeOffset ActiveFrom { get; set; }
    public required DateTimeOffset ActiveUntil { get; set; }
    public bool IsDefault { get; set; }

    public Match MatchesLanguage(Language language)
    {
        return Language?.Matches(language) ?? Match.NoMatch;
    }

    public Match MatchesMediaType(MediaType mediaType)
    {
        return MediaType?.Matches(mediaType) ?? Match.NoMatch;
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
