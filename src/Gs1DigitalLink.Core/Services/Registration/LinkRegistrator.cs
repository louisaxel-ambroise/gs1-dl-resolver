using Gs1DigitalLink.Core.Model;
using Gs1DigitalLink.Core.Services.Conversion;

namespace Gs1DigitalLink.Core.Services.Registration;

public interface ILinkRegistrator
{
    void RegisterLink(Identifier prefix, string redirectUrl, string title, Language? language, DateRange applicability, IEnumerable<string> linkTypes);
    void DeleteLink(Identifier prefix, Language? language, IEnumerable<string> linkTypes);
}

internal sealed class LinkRegistrator(IUserContext userContext, ResolverContext context, TimeProvider timeProvider) : ILinkRegistrator
{
    public void RegisterLink(Identifier identifier, string redirectUrl, string title, Language? language, DateRange applicability, IEnumerable<string> linkTypes)
    {
        var prefix = context.Prefixes
            .Where(c => c.CompanyPrefix == userContext.CompanyPrefix)
            .Where(p => p.Value == identifier.Value)
            .SingleOrDefault();

        if (prefix is null)
        {
            prefix = new Prefix(userContext.CompanyPrefix, identifier.Value);
            context.Prefixes.Add(prefix);
        }

        foreach(var linkType in linkTypes)
        {
            prefix.AddLink(new Link
            {
                Availability = applicability,
                Language = language,
                LinkType = linkType,
                RedirectUrl = redirectUrl,
                Title = title
            });
        }
    }

    public void DeleteLink(Identifier identifier, Language? language, IEnumerable<string> linkTypes)
    {
        var now = timeProvider.GetUtcNow();
        var prefix = context.Prefixes
            .Where(c => c.CompanyPrefix == userContext.CompanyPrefix)
            .Where(p => p.Value == identifier.Value)
            .SingleOrDefault();

        if (prefix is null) return;
     
        prefix.DeactivateLinks(language, linkTypes, now);
    }
}
