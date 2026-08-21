using Goto.Services.Data.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Goto.Infrastructure.Routing.Binding;

internal class FromQueryModelBinder : IModelBinder
{
    public string? Name { get; set; }

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var request = bindingContext.HttpContext.Request;

        if (bindingContext.ModelType == typeof(LinkType))
        {
            var queryParameter = request.Query["linkType"];
            var linkType = queryParameter.LastOrDefault(string.Empty);

            bindingContext.Result = ModelBindingResult.Success(new LinkType(linkType));
        }
        else
        {
            var queryParameter = request.Query[Name ?? bindingContext.FieldName];
            bindingContext.Result = ModelBindingResult.Success(queryParameter);
        }
    }
}