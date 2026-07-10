using DigitalLinkToolkit.Conversion.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Sqids;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Goto.Infrastructure.Authentication;

public sealed class ApiKeyAuthenticationSchemeHandler(IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder urlEncoder, SqidsEncoder<int> idEncoder) 
    : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>(options, logger, urlEncoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var apiKey = Context.Request.Headers["x-api-key"];
        var keyParts = (apiKey.FirstOrDefault() ?? string.Empty).Split('.');

        if (keyParts.Length != 2 || keyParts[0] != Options.ApiKey || !TryExtractCompanyPrefix(keyParts[1], out var gcp))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Api Key header"));
        }

        var claims = new[] 
        { 
            new Claim(ClaimTypes.Name, "Admin"),
            new Claim("gs1:gcp", gcp)
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private bool TryExtractCompanyPrefix(string gcpPart, out string result)
    {
        var decodedValue = idEncoder.Decode(gcpPart);
        result = decodedValue.SingleOrDefault().ToString();

        return CompanyPrefix.Validate(result);
    }
}