using Microsoft.AspNetCore.Mvc.Razor;

namespace Goto.Infrastructure.Views;

public sealed class ViewLocationExpander : IViewLocationExpander
{
    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        var locations = new[] { "/Views/{2}/{1}/{0}.cshtml" };

        return locations.Union(viewLocations);
    }

    public void PopulateValues(ViewLocationExpanderContext context)
    {
        context.Values["customviewlocation"] = nameof(ViewLocationExpander);
    }
}