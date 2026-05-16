import 'package:equatable/equatable.dart';

enum QuestState {
  active,
  readyToClaim,
  claimed,
  expired,
  unknown;

  static QuestState fromString(String value) {
    switch (value) {
      case 'Active':
        return QuestState.active;
      case 'ReadyToClaim':
        return QuestState.readyToClaim;
      case 'Claimed':
        return QuestState.claimed;
      case 'Expired':
        return QuestState.expired;
      default:
        return QuestState.unknown;
    }
  }
}

class PlayerQuest extends Equatable {
  const PlayerQuest({
    required this.id,
    required this.playerId,
    required this.questType,
    required this.state,
    required this.progress,
    required this.goal,
    required this.rewardAmount,
    required this.issuedAt,
    required this.completedAt,
    required this.claimedAt,
    required this.expiresAt,
  });

  factory PlayerQuest.fromJson(Map<String, dynamic> json) {
    final id = json['id'];
    final playerId = json['playerId'];
    final questType = json['questType'];
    final state = json['state'];
    final progress = json['progress'];
    final goal = json['goal'];
    final rewardAmount = json['rewardAmount'];
    final issuedAt = json['issuedAt'];
    final completedAt = json['completedAt'];
    final claimedAt = json['claimedAt'];
    final expiresAt = json['expiresAt'];

    if (id is! String ||
        id.isEmpty ||
        playerId is! String ||
        playerId.isEmpty ||
        questType is! String ||
        questType.isEmpty ||
        state is! String ||
        state.isEmpty ||
        progress is! int ||
        goal is! int ||
        rewardAmount is! int ||
        issuedAt is! String ||
        (completedAt != null && completedAt is! String) ||
        (claimedAt != null && claimedAt is! String) ||
        (expiresAt != null && expiresAt is! String)) {
      throw StateError('Player quest response is missing required fields.');
    }

    return PlayerQuest(
      id: id,
      playerId: playerId,
      questType: questType,
      state: QuestState.fromString(state),
      progress: progress,
      goal: goal,
      rewardAmount: rewardAmount,
      issuedAt: DateTime.parse(issuedAt),
      completedAt: completedAt == null
          ? null
          : DateTime.parse(completedAt as String),
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
  final String questType;
  final QuestState state;
  final int progress;
  final int goal;
  final int rewardAmount;
  final DateTime issuedAt;
  final DateTime? completedAt;
  final DateTime? claimedAt;
  final DateTime? expiresAt;

  bool get isReadyToClaim => state == QuestState.readyToClaim;
  bool get isClosed =>
      state == QuestState.claimed || state == QuestState.expired;

  @override
  List<Object?> get props => [
    id,
    playerId,
    questType,
    state,
    progress,
    goal,
    rewardAmount,
    issuedAt,
    completedAt,
    claimedAt,
    expiresAt,
  ];
}
