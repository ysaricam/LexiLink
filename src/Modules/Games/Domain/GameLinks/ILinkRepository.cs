namespace LexiLink.Modules.Games.Domain.GameLinks;

public interface ILinkRepository
{
    Task AddAsync(Link link);
    
    Task<Link?> GetByIdAsync(LinkId id);
}