import 'package:equatable/equatable.dart';

class PlayerDiamond extends Equatable {
  const PlayerDiamond({
    required this.playerId,
    required this.balance,
  });

  factory PlayerDiamond.fromJson(Map<String, dynamic> json) {
    final playerId = json['playerId'];
    final balance = json['balance'];

    if (playerId is! String || playerId.isEmpty || balance is! int) {
      throw StateError('Player diamond response is missing required fields.');
    }

    return PlayerDiamond(playerId: playerId, balance: balance);
  }

  final String playerId;
  final int balance;

  @override
  List<Object?> get props => [playerId, balance];
}
