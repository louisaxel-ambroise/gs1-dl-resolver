using Goto.Services.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace Goto.Infrastructure.Authentication;

public sealed class ApiKeyAuthenticationSchemeHandler(IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder urlEncoder, Context context) 
    : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>(options, logger, urlEncoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var endpoint = Context.GetEndpoint();
        if (endpoint is null || endpoint.Metadata.GetMetadata<AuthorizeAttribute>() is null)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var apiKey = Context.Request.Headers[HeaderKey].FirstOrDefault(string.Empty);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
        var keyDetails = context.GetApiKeyDetails(Convert.ToBase64String(hash));

        if(keyDetails is null || keyDetails.BeginValidityDate > TimeProvider.GetUtcNow())
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }
        if (keyDetails.EndValidityDate < TimeProvider.GetUtcNow())
        {
            return Task.FromResult(AuthenticateResult.Fail("Expired API Key"));
        }

        var claims = new[] 
        { 
            new Claim(ClaimTypes.Name, keyDetails.Name),
            new Claim("gs1:gcp", keyDetails.CompanyPrefix)
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    const string HeaderKey = "x-api-key";
}