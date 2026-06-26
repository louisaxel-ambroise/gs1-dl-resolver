using Goto.Services.Conversion.Utils.Validation;
using System.ComponentModel.DataAnnotations;

namespace Goto.Infrastructure.Routing.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class CompanyPrefixAttribute() : ValidationAttribute(DefaultErrorMessageString)
{
    private const string DefaultErrorMessageString = "Company prefix is not valid";

    public override bool IsValid(object? value)
    {
        return value is string str && CompanyPrefix.Validate(str);
    }
}
