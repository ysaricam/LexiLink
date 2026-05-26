import 'package:equatable/equatable.dart';

class PlayerReset extends Equatable {
  const PlayerReset({
    required this.playerId,
    required this.balance,
  });

  factory PlayerReset.fromJson(Map<String, dynamic> json) {
    final playerId = json['playerId'];
    final balance = json['balance'];

    if (playerId is! String || playerId.isEmpty || balance is! int) {
      throw StateError('Player reset response is missing required fields.');
    }

    return PlayerReset(playerId: playerId, balance: balance);
  }

  final String playerId;
  final int balance;

  @override
  List<Object?> get props => [playerId, balance];
}
