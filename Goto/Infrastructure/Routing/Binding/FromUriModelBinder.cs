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
            var digitalLink = converter.Parse(request);

            bindingContext.HttpContext.Items.Add("gs1:gcp", digitalLink.CompanyPrefix);
            bindingContext.HttpContext.Items.Add("gs1:digitalLink", digitalLink);
            bindingContext.Result = ModelBindingResult.Success(digitalLink);
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
