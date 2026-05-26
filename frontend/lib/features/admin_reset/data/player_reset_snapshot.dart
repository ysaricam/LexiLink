class PlayerResetSnapshot {
  const PlayerResetSnapshot({
    required this.playerId,
    required this.balance,
  });

  factory PlayerResetSnapshot.fromJson(Map<String, dynamic> json) {
    return PlayerResetSnapshot(
      playerId: json['playerId'] as String,
      balance: json['balance'] as int,
    );
  }

  final String playerId;
  final int balance;
}
