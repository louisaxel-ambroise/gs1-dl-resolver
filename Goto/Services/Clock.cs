namespace Goto.Services;

public sealed class Clock
{
    internal void SetNow(DateTimeOffset requestDate) => Now = requestDate;

    public DateTimeOffset Now { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UtcNow => Now.ToUniversalTime();
}