import 'package:lexilink_app/features/admin_quests/data/quest_enums.dart';

class QuestDefinition {
  const QuestDefinition({
    required this.id,
    required this.questType,
    required this.cadence,
    required this.goal,
    required this.rewardAmount,
    required this.isActive,
    this.prerequisiteQuestType,
  });

  factory QuestDefinition.fromJson(Map<String, dynamic> json) {
    return QuestDefinition(
      id: json['id'] as String,
      questType: AdminQuestType.fromWire(json['questType'] as String),
      cadence: AdminQuestCadence.fromWire(json['cadence'] as String),
      goal: json['goal'] as int,
      rewardAmount: json['rewardAmount'] as int,
      prerequisiteQuestType:
          AdminQuestType.tryFromWire(json['prerequisiteQuestType'] as String?),
      isActive: json['isActive'] as bool,
    );
  }

  final String id;
  final AdminQuestType questType;
  final AdminQuestCadence cadence;
  final int goal;
  final int rewardAmount;
  final AdminQuestType? prerequisiteQuestType;
  final bool isActive;
}
