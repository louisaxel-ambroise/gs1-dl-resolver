using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace Goto.Infrastructure;

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

        var viewName = context.ObjectType!.Namespace![5..].Replace('.', '/') + "/" + context.ObjectType.Name + ".cs.cshtml";
        var viewResult = engine.GetView("", viewName, true);
        if (!viewResult.Success)
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
