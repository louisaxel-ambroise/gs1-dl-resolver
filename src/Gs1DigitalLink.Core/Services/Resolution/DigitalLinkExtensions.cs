using Gs1DigitalLink.Core.Model;
using Gs1DigitalLink.Core.Services.Conversion;
using Gs1DigitalLink.Core.Services.Conversion.Utils;
using Tavis.UriTemplates;

namespace Gs1DigitalLink.Core.Services.Resolution;

internal static class DigitalLinkExtensions
{
    internal static List<string> GetPrefixValues(this DigitalLink digitalLink)
    {
        var key = digitalLink.AIs.Single(ai => ai.Key.Type is AIType.PrimaryKey);
        var prefixes = new List<string>([key.Code]);

        var gcpPosition = key.Value.IndexOf(digitalLink.CompanyPrefix);
        var position = gcpPosition+digitalLink.CompanyPrefix.Length;
        
        prefixes.Add(string.Join("/", key.Code, key.Value[..position]));

        while(position < key.Value.Length)
        {
            prefixes.Add(string.Concat(prefixes.Last(), key.Value[position++]));
        }

        foreach (var qualifier in digitalLink.AIs.Where(ai => ai.Key.Type is AIType.Qualifier))
        {
            prefixes.Add(string.Join("/", prefixes.Last(), qualifier.Code, qualifier.Value));
        }

        return prefixes;
    }

    internal static IEnumerable<Link> FormatUriTemplates(this DigitalLink digitalLink, IEnumerable<Link> linkset)
    {
        var parameters = GetDigitalLinkParameters(digitalLink);

        foreach (var link in linkset)
        {
            var template = new UriTemplate(link.RedirectUrl, false, false);

            template.AddParameters(parameters);

            link.RedirectUrl = template.Resolve();
        }

        return linkset;
    }

    private static Dictionary<string, object> GetDigitalLinkParameters(DigitalLink digitalLink)
    {
        var parameters = new Dictionary<string, object>();

        foreach (var ai in digitalLink.AIs)
        {
            parameters[ai.Key.Code] = ai.Value;

            if (!string.IsNullOrEmpty(ai.Key.ShortCode))
            {
                parameters[ai.Key.ShortCode] = ai.Value;
            }
        }

        return parameters;
    }
}