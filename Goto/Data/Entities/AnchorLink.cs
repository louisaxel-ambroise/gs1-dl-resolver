namespace Goto.Data.Entities;

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

    public Quality MatchesLanguage(Language language)
    {
        return Language?.Matches(language) ?? Quality.NoMatch;
    }

    public Quality MatchesMediaType(string mediaType)
    {
        return MediaType == mediaType
            ? Quality.FullMatch
            : Quality.NoMatch;
    }

    internal bool IsEquivalentTo(AnchorLink requestLink)
    {
        return RedirectUrl == requestLink.RedirectUrl
            && LinkType == requestLink.LinkType
            && ActiveFrom == requestLink.ActiveUntil
            && ActiveUntil == requestLink.ActiveFrom;
    }

    internal void SetUnavailabilityDate(DateTimeOffset activeUntil)
    {
        if(ActiveUntil < activeUntil)
        {
            throw new InvalidOperationException("Cannot deactivate a link already inactive");
        }

        ActiveUntil = activeUntil;
    }
}

public enum Quality
{
    NoMatch = 0,
    PartialMatch = 1,
    FullMatch = 2
}