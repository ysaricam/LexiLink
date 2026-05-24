using LexiLink.Modules.Quests.Domain.PlayerQuests;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Modules.Quests.Infrastructure.Domain.PlayerQuests;

internal sealed class QuestDefinitionRepository : IQuestDefinitionRepository
{
    private readonly QuestsContext _context;

    internal QuestDefinitionRepository(QuestsContext context)
    {
        _context = context;
    }

    public Task<QuestDefinition?> GetByIdAsync(QuestDefinitionId id, CancellationToken cancellationToken = default) =>
        _context.QuestDefinitions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<QuestDefinition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _context.QuestDefinitions
            .OrderBy(x => EF.Property<string>(x, "_name"))
            .ToListAsync(cancellationToken);
        return list;
    }

    public async Task AddAsync(QuestDefinition definition, CancellationToken cancellationToken = default)
    {
        await _context.QuestDefinitions.AddAsync(definition, cancellationToken);
    }
}
