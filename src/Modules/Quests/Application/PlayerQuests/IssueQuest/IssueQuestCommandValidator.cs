using FluentValidation;

namespace LexiLink.Modules.Quests.Application.PlayerQuests.IssueQuest;

internal class IssueQuestCommandValidator : AbstractValidator<IssueQuestCommand>
{
    public IssueQuestCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.QuestDefinitionId).NotEmpty();
    }
}
