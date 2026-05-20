using MediatR;
using LexiLink.Modules.Quests.Application.Configuration.Commands;
using LexiLink.Modules.Quests.Application.PlayerQuests.IssueQuest;

namespace LexiLink.Modules.Quests.Application.Admin.PlayerQuests.IssueQuestToPlayer;

internal sealed class IssueQuestToPlayerCommandHandler : ICommandHandler<IssueQuestToPlayerCommand>
{
    private readonly ISender _sender;

    internal IssueQuestToPlayerCommandHandler(ISender sender)
    {
        _sender = sender;
    }

    public Task Handle(IssueQuestToPlayerCommand request, CancellationToken cancellationToken)
    {
        // Wrap the internal IssueQuestCommand so the admin entry point
        // reuses the exact prerequisite / cadence / idempotency logic
        // already covered by Quests unit + IT.
        return _sender.Send(new IssueQuestCommand(request.PlayerId, request.QuestType), cancellationToken);
    }
}
