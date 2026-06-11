using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Goto.Infrastructure.Binding;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class FromUriAttribute : Attribute, IBindingSourceMetadata, IModelNameProvider
{
    public BindingSource BindingSource => BindingSource.Custom;
    public string? Name { get; set; }
}
