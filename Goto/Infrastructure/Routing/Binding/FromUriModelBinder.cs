using Goto.Services.Conversion;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Goto.Infrastructure.Routing.Binding;

public sealed class FromUriModelBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var request = bindingContext.HttpContext.Request;

        if (bindingContext.ModelType == typeof(DigitalLink))
        {
            var converter = bindingContext.HttpContext.RequestServices.GetRequiredService<DigitalLinkConverter>();
            var digitalLink = converter.Parse(string.Concat(request.Scheme, "://", request.Host), request.Path, request.QueryString.ToString());

            bindingContext.Result = ModelBindingResult.Success(digitalLink);
        }
        else if (bindingContext.ModelType == typeof(string))
        {
            bindingContext.Result = ModelBindingResult.Success(request.GetDisplayUrl());
        }
        else if (bindingContext.ModelType == typeof(Uri))
        {
            bindingContext.Result = ModelBindingResult.Success(new Uri(request.GetDisplayUrl()));
        }
        else
        {
            bindingContext.Result = ModelBindingResult.Failed();
        }
    }
}
