namespace Goto.Services;

public static class DateTimeOffsetExtensions
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
}
