namespace LexiLink.Modules.Players.Domain.Players;

public interface IDiscriminatorGenerator
{
    Task<Discriminator> GenerateForAsync(string displayName, CancellationToken cancellationToken = default);
}
