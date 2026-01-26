using Gs1DigitalLink.Core.Services.Registration;

namespace Gs1DigitalLink.Api.Services;

public class HttpHeaderUserContext(IHttpContextAccessor contextAccessor) : IUserContext
{
    public string Name => "HTTP Header User";
    public string CompanyPrefix => contextAccessor.HttpContext?.Request.Headers["X-Company-Prefix"].FirstOrDefault() 
        ?? throw new InvalidOperationException("X-Company-Prefix not set");
}
