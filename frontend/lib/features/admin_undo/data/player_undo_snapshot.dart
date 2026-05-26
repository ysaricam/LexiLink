class PlayerUndoSnapshot {
  const PlayerUndoSnapshot({
    required this.playerId,
    required this.balance,
  });

  factory PlayerUndoSnapshot.fromJson(Map<String, dynamic> json) {
    return PlayerUndoSnapshot(
      playerId: json['playerId'] as String,
      balance: json['balance'] as int,
    );
  }

  final String playerId;
  final int balance;
}
