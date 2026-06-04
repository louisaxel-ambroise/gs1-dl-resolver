using Gs1DigitalLink.Core.Contracts.Registration;
using Gs1DigitalLink.Core.Model;
using Gs1DigitalLink.Core.Services.Conversion;
using Gs1DigitalLink.Core.Services.Conversion.Utils.Validation;

namespace Gs1DigitalLink.Core.Services.Registration;

internal sealed class LinkRegistrator(IUserContext userContext, ResolverContext context, TimeProvider timeProvider) : ILinkRegistrator
{
    public void RegisterLink(Identifier identifier, string redirectUrl, string title, Language? language, DateRange applicability, IEnumerable<string> linkTypes)
    {
        if(CompanyPrefix.GetCompanyPrefixLength(userContext.CompanyPrefix) != userContext.CompanyPrefix.Length)
        {
            throw new InvalidOperationException("Company prefix is invalid");
        }
        if (identifier.CompanyPrefix is not null && !Equals(userContext.CompanyPrefix, identifier.CompanyPrefix))
        {
            throw new InvalidOperationException("Company prefix does not match the AI to be registered");
        }

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
        if (CompanyPrefix.GetCompanyPrefixLength(userContext.CompanyPrefix) != userContext.CompanyPrefix.Length)
        {
            throw new InvalidOperationException("Company prefix is invalid");
        }
        if (identifier.CompanyPrefix is not null && !Equals(userContext.CompanyPrefix, identifier.CompanyPrefix))
        {
            throw new InvalidOperationException("Company prefix does not match the AI to be removed");
        }

        var now = timeProvider.GetUtcNow();
        var prefix = context.Prefixes
            .Where(c => c.CompanyPrefix == userContext.CompanyPrefix)
            .Where(p => p.Value == identifier.Value)
            .SingleOrDefault();

        if (prefix is null) return;
     
        prefix.DeactivateLinks(language, linkTypes, now);
    }
}
