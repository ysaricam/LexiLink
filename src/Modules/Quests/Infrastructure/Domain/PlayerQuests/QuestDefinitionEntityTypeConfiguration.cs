using LexiLink.Modules.Quests.Domain.PlayerQuests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Quests.Infrastructure.Domain.PlayerQuests;

internal sealed class QuestDefinitionEntityTypeConfiguration : IEntityTypeConfiguration<QuestDefinition>
{
    public void Configure(EntityTypeBuilder<QuestDefinition> builder)
    {
        builder.ToTable("QuestDefinitions", "quests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id");

        builder.Property<string>("_name")
            .HasColumnName("Name")
            .HasMaxLength(64);

        builder.Property<string>("_description")
            .HasColumnName("Description")
            .HasMaxLength(256);

        builder.Property<QuestTrigger>("_trigger")
            .HasColumnName("Trigger")
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property<int>("_threshold").HasColumnName("Threshold");
        builder.Property<int>("_energyReward").HasColumnName("EnergyReward");
        builder.Property<int>("_hintReward").HasColumnName("HintReward");

        builder.Property<QuestDefinitionId?>("_prerequisiteQuestDefinitionId")
            .HasColumnName("PrerequisiteQuestDefinitionId");

        builder.Property<ProgressBaseline>("_progressBaseline")
            .HasColumnName("ProgressBaseline")
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property<bool>("_isActive").HasColumnName("IsActive");
    }
}
