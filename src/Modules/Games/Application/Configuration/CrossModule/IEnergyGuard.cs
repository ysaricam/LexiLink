namespace LexiLink.Modules.Games.Application.Configuration.CrossModule;

// Cross-module gateway from Games to the Energy module. The interface lives in
// Games.Application so Games depends only on its own surface; the implementation
// is composed in the API host (see LexiLink.API.CrossModule.EnergyGuard) and
// translates the request into an Energy module command.
//
// This is the first synchronous cross-module dependency in LexiLink. Documented
// as an intentional deviation in docs/kamil-modular-monolith-comparison.md.
public interface IEnergyGuard
{
    Task EnsureCanStartGameAsync(Guid playerId, CancellationToken cancellationToken = default);
}
