using Gs1DigitalLink.Core;

namespace Gs1DigitalLink.Web;

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
         string[] AffectedMethods = [HttpMethod.Post.Method, HttpMethod.Put.Method, HttpMethod.Patch.Method, HttpMethod.Delete.Method];
        
        return app.Use(async (req, next) =>
        {
            await next();

            if (AffectedMethods.Contains(req.Request.Method))
            {
                var context = req.RequestServices.GetRequiredService<ResolverContext>();
                context.SaveChanges();
            }
        });
    }
}