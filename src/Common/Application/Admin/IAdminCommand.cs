namespace LexiLink.Common.Application.Admin;

/// <summary>
/// Marker interface for application commands that require an authorized
/// admin principal. Each consumer module's command handlers may also
/// implement <see cref="IAdminAuthorizationContext"/> checks; B5 will
/// add an auditing decorator that discovers admin commands through this
/// marker.
/// </summary>
public interface IAdminCommand
{
}
