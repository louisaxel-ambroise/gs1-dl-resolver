using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Goto.Infrastructure.Routing.Binding;

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
        if (context.BindingInfo.BindingSource?.CanAcceptDataFrom(BindingSource.Query) ?? false)
        {
            return new FromQueryModelBinder();
        }
        else
        {
            return null;
        }
    }
}
