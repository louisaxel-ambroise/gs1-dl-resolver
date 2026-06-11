using Goto.Data.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Goto.Infrastructure.Binding;

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
                .Select(h => new Language(h.Value.ToString()))
                .ToArray();

            bindingContext.Result = ModelBindingResult.Success(languages);
        }
        else if(bindingContext.ModelType == typeof(string[]))
        {
            var header = request.GetTypedHeaders().Accept;
            var mediaTypes = header.OrderByDescending(h => h.Quality)
                .Select(h => h.MediaType.ToString())
                .ToArray();

            bindingContext.Result = ModelBindingResult.Success(mediaTypes);
        }
        else if (bindingContext.ModelType == typeof(string) && !string.IsNullOrEmpty(Name))
        {
            var header = request.Headers[Name];
            bindingContext.Result = ModelBindingResult.Success(header.FirstOrDefault());
        }
    }
}