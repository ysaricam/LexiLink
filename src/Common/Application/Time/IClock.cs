namespace LexiLink.Common.Application.Time;

public interface IClock
{
    DateTime UtcNow { get; }
}
