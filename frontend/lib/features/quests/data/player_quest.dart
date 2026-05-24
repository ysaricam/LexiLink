import 'package:equatable/equatable.dart';

/// Display states the backend computes on each /quests/me sync. The
/// persisted Domain state is only Active|Claimed, but the read pass
/// promotes Active rows to ReadyToClaim when progress >= threshold.
enum QuestState {
  active,
  readyToClaim,
  claimed,
  unknown;

  static QuestState fromString(String value) {
    switch (value) {
      case 'Active':
        return QuestState.active;
      case 'ReadyToClaim':
        return QuestState.readyToClaim;
      case 'Claimed':
        return QuestState.claimed;
      default:
        return QuestState.unknown;
    }
  }
}

class PlayerQuest extends Equatable {
  const PlayerQuest({
    required this.id,
    required this.playerId,
    required this.questDefinitionId,
    required this.name,
    required this.description,
    required this.trigger,
    required this.state,
    required this.progress,
    required this.threshold,
    required this.reward,
    required this.issuedAt,
    required this.claimedAt,
    required this.expiresAt,
  });

  factory PlayerQuest.fromJson(Map<String, dynamic> json) {
    final id = json['id'];
    final playerId = json['playerId'];
    final questDefinitionId = json['questDefinitionId'];
    final name = json['name'];
    final description = json['description'];
    final trigger = json['trigger'];
    final displayState = json['displayState'];
    final progress = json['progress'];
    final threshold = json['threshold'];
    final reward = json['reward'];
    final issuedAt = json['issuedAt'];
    final claimedAt = json['claimedAt'];
    final expiresAt = json['expiresAt'];

    if (id is! String ||
        id.isEmpty ||
        playerId is! String ||
        playerId.isEmpty ||
        questDefinitionId is! String ||
        questDefinitionId.isEmpty ||
        name is! String ||
        name.isEmpty ||
        trigger is! String ||
        trigger.isEmpty ||
        displayState is! String ||
        displayState.isEmpty ||
        progress is! int ||
        threshold is! int ||
        reward is! int ||
        issuedAt is! String ||
        (description != null && description is! String) ||
        (claimedAt != null && claimedAt is! String) ||
        (expiresAt != null && expiresAt is! String)) {
      throw StateError('Player quest response is missing required fields.');
    }

    return PlayerQuest(
      id: id,
      playerId: playerId,
      questDefinitionId: questDefinitionId,
      name: name,
      description: (description as String?) ?? '',
      trigger: trigger,
      state: QuestState.fromString(displayState),
      progress: progress,
      threshold: threshold,
      reward: reward,
      issuedAt: DateTime.parse(issuedAt),
      claimedAt: claimedAt == null
          ? null
          : DateTime.parse(claimedAt as String),
      expiresAt: expiresAt == null
          ? null
          : DateTime.parse(expiresAt as String),
    );
  }

  final String id;
  final String playerId;
  final String questDefinitionId;
  final String name;
  final String description;
  final String trigger;
  final QuestState state;
  final int progress;
  final int threshold;
  final int reward;
  final DateTime issuedAt;
  final DateTime? claimedAt;
  final DateTime? expiresAt;

  bool get isReadyToClaim => state == QuestState.readyToClaim;
  bool get isClosed => state == QuestState.claimed;

  @override
  List<Object?> get props => [
    id,
    playerId,
    questDefinitionId,
    name,
    description,
    trigger,
    state,
    progress,
    threshold,
    reward,
    issuedAt,
    claimedAt,
    expiresAt,
  ];
}
