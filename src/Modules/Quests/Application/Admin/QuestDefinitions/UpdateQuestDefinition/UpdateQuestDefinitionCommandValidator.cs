using FluentValidation;

namespace LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.UpdateQuestDefinition;

internal sealed class UpdateQuestDefinitionCommandValidator : AbstractValidator<UpdateQuestDefinitionCommand>
{
    public UpdateQuestDefinitionCommandValidator()
    {
        RuleFor(x => x.QuestDefinitionId).NotEmpty();
        RuleFor(x => x.Goal).GreaterThan(0);
        RuleFor(x => x.RewardAmount).GreaterThan(0);
    }
}
