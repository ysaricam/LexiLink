import 'package:lexilink_app/features/admin_quests/data/quest_enums.dart';

class QuestDefinition {
  const QuestDefinition({
    required this.id,
    required this.name,
    required this.description,
    required this.trigger,
    required this.threshold,
    required this.energyReward,
    required this.hintReward,
    required this.progressBaseline,
    required this.isActive,
    this.prerequisiteQuestDefinitionId,
  });

  factory QuestDefinition.fromJson(Map<String, dynamic> json) {
    return QuestDefinition(
      id: json['id'] as String,
      name: json['name'] as String,
      description: (json['description'] as String?) ?? '',
      trigger: QuestTrigger.fromWire(json['trigger'] as String),
      threshold: json['threshold'] as int,
      energyReward: json['energyReward'] as int,
      hintReward: json['hintReward'] as int,
      prerequisiteQuestDefinitionId:
          json['prerequisiteQuestDefinitionId'] as String?,
      progressBaseline:
          ProgressBaseline.fromWire(json['progressBaseline'] as String),
      isActive: json['isActive'] as bool,
    );
  }

  final String id;
  final String name;
  final String description;
  final QuestTrigger trigger;
  final int threshold;
  final int energyReward;
  final int hintReward;
  final String? prerequisiteQuestDefinitionId;
  final ProgressBaseline progressBaseline;
  final bool isActive;
}
