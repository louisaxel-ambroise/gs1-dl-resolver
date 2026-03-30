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
            || LinksetLinkTypeValues.Any(x => x.Equals(request.Query["linkType"].ToString(), StringComparison.OrdinalIgnoreCase));
    }

    public static IApplicationBuilder UseUnitOfWork(this IApplicationBuilder app)
    {
        return app.Use(async (req, next) =>
        {
            await next();

            if (CommandHttpMethods.Any(x => x.Equals(req.Request.Method, StringComparison.OrdinalIgnoreCase)))
            {
                var context = req.RequestServices.GetRequiredService<ResolverContext>();
                context.SaveChanges();
            }
        });
    }

    static readonly string[] LinksetLinkTypeValues = ["linkset", "all"];
    static readonly string[] CommandHttpMethods = [HttpMethod.Post.Method, HttpMethod.Put.Method, HttpMethod.Patch.Method, HttpMethod.Delete.Method];
}