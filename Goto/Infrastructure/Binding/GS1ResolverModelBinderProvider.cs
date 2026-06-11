using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Goto.Infrastructure.Binding;

public sealed class GS1ResolverModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.BindingInfo.BindingSource?.CanAcceptDataFrom(BindingSource.Custom) ?? false)
        {
            return new FromUriModelBinder();
        }
        if (context.BindingInfo.BindingSource?.CanAcceptDataFrom(BindingSource.Header) ?? false)
        {
            return new FromHeaderModelBinder();
        }
        else
        {
            return null;
        }
    }
}
