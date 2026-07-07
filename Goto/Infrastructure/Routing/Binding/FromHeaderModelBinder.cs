using Goto.Services.Data.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Goto.Infrastructure.Routing.Binding;

internal class FromHeaderModelBinder : IModelBinder
{
    public string? Name { get; set; }

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var request = bindingContext.HttpContext.Request;

        if (bindingContext.ModelType == typeof(Language[]))
        {
            var header = request.GetTypedHeaders().AcceptLanguage;
            var languages = header.OrderByDescending(h => h.Quality ?? 1)
                .Select(v => Language.Parse(v.Value.ToString()))
                .ToArray();

            bindingContext.Result = ModelBindingResult.Success(languages);
        }
        else if(bindingContext.ModelType == typeof(MediaType[]))
        {
            var header = request.GetTypedHeaders().Accept;
            var mediaTypes = header.OrderByDescending(h => h.Quality ?? 1)
                .Select(h => MediaType.Parse(h.MediaType.ToString()))
                .ToArray();

            bindingContext.Result = ModelBindingResult.Success(mediaTypes);
        }
    }
}

internal class FromQueryModelBinder : IModelBinder
{
    public string? Name { get; set; }

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var request = bindingContext.HttpContext.Request;

        if (bindingContext.ModelType == typeof(LinkType))
        {
            var queryParameter = request.Query["linkType"];
            var linkType = queryParameter.LastOrDefault() ?? string.Empty;

            bindingContext.Result = ModelBindingResult.Success(LinkType.Parse(linkType));
        }
        else
        {
            var queryParameter = request.Query[Name ?? bindingContext.FieldName];
            bindingContext.Result = ModelBindingResult.Success(queryParameter);
        }
    }
}