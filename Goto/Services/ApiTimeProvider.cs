namespace Goto.Services;

public sealed class ApiTimeProvider
{
    internal void SetRequestDate(DateTimeOffset requestDate) => Now = requestDate;

    public DateTimeOffset Now { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UtcNow => Now.ToUniversalTime();
}