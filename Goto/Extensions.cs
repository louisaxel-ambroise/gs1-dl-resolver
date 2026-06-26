namespace Goto;

public static class Extensions
{
    extension (DateTimeOffset)
    {
        public static DateTimeOffset Min(params DateTimeOffset?[] values)
        {
            return values.OrderBy(x => x).FirstOrDefault(x => x is not null) ?? DateTimeOffset.UtcNow;
        }

        public static DateTimeOffset Max(params DateTimeOffset?[] values)
        {
            return values.OrderByDescending(x => x).FirstOrDefault(x => x is not null) ?? DateTimeOffset.UtcNow;
        }
    }

    extension(HttpResponse response)
    {
        public bool IsSuccessStatusCode()
        {
            return response.StatusCode is >= 200 and < 400;
        }
    }
}
