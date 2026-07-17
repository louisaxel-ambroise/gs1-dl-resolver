using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace Goto.Infrastructure.Views;

internal sealed class HtmlViewFormatter : TextOutputFormatter
{
    public HtmlViewFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/html"));
        SupportedEncodings.Add(Encoding.UTF8);
    }

    protected override bool CanWriteType(Type? type) => type?.Namespace is not null && type.Namespace.Equals("Goto.Controllers.Results");

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding encoding)
    {
        var serviceProvider = context.HttpContext.RequestServices;
        var engine = serviceProvider.GetRequiredService<IRazorViewEngine>();
        var tempData = serviceProvider.GetRequiredService<ITempDataProvider>();

        var actionContext = new ActionContext(
            context.HttpContext,
            new RouteData(),
            new ActionDescriptor());

        var viewName = context.ObjectType!.Namespace![5..].Replace('.', '/') + "/" + context.ObjectType.Name + ".cshtml";
        var viewResult = engine.GetView("", viewName, false);
        
        if (viewResult is null || !viewResult.Success)
        {
            throw new InvalidOperationException($"View '{context.ObjectType!.Name}' not found.");
        }

        await using var sw = new StringWriter();

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = context.Object },
            new TempDataDictionary(context.HttpContext, tempData),
            sw,
            new HtmlHelperOptions());

        await viewResult.View.RenderAsync(viewContext);
        await context.HttpContext.Response.WriteAsync(sw.ToString());
    }
}

public class ViewLocationExpander : IViewLocationExpander
{
    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        //{2} is area, {1} is controller,{0} is the action
        string[] locations = new string[] { "/Views/{2}/{1}/{0}.cshtml" };
        return locations.Union(viewLocations);          //Add mvc default locations after ours
    }

    public void PopulateValues(ViewLocationExpanderContext context)
    {
        context.Values["customviewlocation"] = nameof(ViewLocationExpander);
    }
}