using System.Security.Claims;

namespace Goto.Services;

public static class Extensions
{
    extension (DateTimeOffset)
    {
        public static DateTimeOffset Min(params DateTimeOffset?[] values)
        {
            return values.OrderBy(x => x).FirstOrDefault(x => x is not null, DateTimeOffset.UtcNow).Value;
        }

        public static DateTimeOffset Max(params DateTimeOffset?[] values)
        {
            return values.OrderBy(x => x).LastOrDefault(x => x is not null, DateTimeOffset.UtcNow).Value;
        }
    }

    extension(HttpResponse response)
    {
        public bool IsSuccessStatusCode()
        {
            return response.StatusCode is >= 200 and < 400;
        }
    }

    extension(ClaimsPrincipal principal)
    {
        public string GetCompanyPrefix() => principal.FindFirstValue("gs1:gcp") ?? throw new InvalidOperationException("Principal does not have company prefix claim");
    }
}
