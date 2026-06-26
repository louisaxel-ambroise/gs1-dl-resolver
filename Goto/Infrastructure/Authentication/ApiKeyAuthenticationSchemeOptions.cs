using Microsoft.AspNetCore.Authentication;

namespace Goto.Infrastructure.Authentication;

public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public string? ApiKey { get; set; }

    public static string ConfigurationSection = "Auth";
}
