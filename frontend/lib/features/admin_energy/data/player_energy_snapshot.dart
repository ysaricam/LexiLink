class PlayerEnergySnapshot {
  const PlayerEnergySnapshot({
    required this.playerId,
    required this.currentAmount,
    required this.maximumAmount,
    required this.isFull,
    required this.rechargeIntervalSeconds,
    required this.lastRefilledOn,
    this.secondsUntilNextRefill,
    this.fullyRefilledAt,
  });

  factory PlayerEnergySnapshot.fromJson(Map<String, dynamic> json) {
    return PlayerEnergySnapshot(
      playerId: json['playerId'] as String,
      currentAmount: json['currentAmount'] as int,
      maximumAmount: json['maximumAmount'] as int,
      isFull: json['isFull'] as bool,
      rechargeIntervalSeconds: json['rechargeIntervalSeconds'] as int,
      lastRefilledOn: DateTime.parse(json['lastRefilledOn'] as String),
      secondsUntilNextRefill: json['secondsUntilNextRefill'] as int?,
      fullyRefilledAt: json['fullyRefilledAt'] == null
          ? null
          : DateTime.parse(json['fullyRefilledAt'] as String),
    );
  }

  final String playerId;
  final int currentAmount;
  final int maximumAmount;
  final bool isFull;
  final int rechargeIntervalSeconds;
  final DateTime lastRefilledOn;
  final int? secondsUntilNextRefill;
  final DateTime? fullyRefilledAt;
}
