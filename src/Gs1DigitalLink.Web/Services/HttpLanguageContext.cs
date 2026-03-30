using Gs1DigitalLink.Core.Contracts.Resolution;
using Gs1DigitalLink.Core.Services.Resolution;
using System.Net.Http.Headers;

namespace Gs1DigitalLink.Web.Services;

internal sealed class HttpLanguageContext(IHttpContextAccessor contextAccessor) : ILanguageContext
{
    public IEnumerable<LanguagePreference> GetLanguages()
    {
        var context = contextAccessor.HttpContext;

        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var header = context.Request.Headers.AcceptLanguage.ToString();

        if (!string.IsNullOrWhiteSpace(header))
        {
            return header
                .Split(',')
                .Select(StringWithQualityHeaderValue.Parse)
                .Select(ParseLanguagePreference);
        }
        else
        {
            return [];
        }
    }

    private static LanguagePreference ParseLanguagePreference(StringWithQualityHeaderValue value)
    {
        var parts = value.Value.Split('-');

        return new()
        {
            Language = parts[0],
            Region = parts.Length > 1 ? parts[1] : null,
            Quality = value.Quality ?? 1.0
        };
    }
}