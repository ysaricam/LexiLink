using LexiLink.Common.Application.Time;

namespace LexiLink.Common.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
