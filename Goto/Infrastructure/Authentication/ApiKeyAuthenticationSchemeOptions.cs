using Microsoft.AspNetCore.Authentication;

namespace Goto.Infrastructure.Authentication;

public sealed class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public string? ApiKey { get; set; }
}
