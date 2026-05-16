using FluentValidation;

namespace LexiLink.Modules.Quests.Application.PlayerQuests.RecordQuestProgress;

internal class RecordQuestProgressCommandValidator : AbstractValidator<RecordQuestProgressCommand>
{
    public RecordQuestProgressCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.QuestType).IsInEnum();
        RuleFor(x => x.Delta).GreaterThan(0);
    }
}
