class PlayerHintSnapshot {
  const PlayerHintSnapshot({
    required this.playerId,
    required this.balance,
  });

  factory PlayerHintSnapshot.fromJson(Map<String, dynamic> json) {
    return PlayerHintSnapshot(
      playerId: json['playerId'] as String,
      balance: json['balance'] as int,
    );
  }

  final String playerId;
  final int balance;
}
