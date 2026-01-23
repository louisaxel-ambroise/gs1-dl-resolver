using Gs1DigitalLink.Core;

namespace Gs1DigitalLink.Api;

public static class Extensions
{
    public static DateTimeOffset GetApplicableDate(this HttpRequest request)
    {
        if (request.Headers.TryGetValue("X-Applicability-Date", out var values) && DateTimeOffset.TryParse(values.FirstOrDefault(), out var date))
        {
            return date;
        }

        return TimeProvider.System.GetUtcNow();
    }

    public static bool IsLinksetRequested(this HttpRequest request)
    {
        return request.GetTypedHeaders().Accept.Any(a => a.MediaType == "application/linkset+json")
            || request.Query["linkType"].ToString() is "linkset" or "all";
    }

    public static IApplicationBuilder UseUnitOfWork(this IApplicationBuilder app)
    {
        return app.Use(async (req, next) =>
        {
            var methods = new[] { HttpMethod.Post.Method, HttpMethod.Put.Method, HttpMethod.Patch.Method, HttpMethod.Delete.Method };
            await next();

            if (methods.Contains(req.Request.Method))
            {
                var context = req.RequestServices.GetRequiredService<DigitalLinkContext>();
                context.SaveChanges();
            }
        });
    }
}