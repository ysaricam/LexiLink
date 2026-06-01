import 'package:equatable/equatable.dart';

/// The player's rewarded-ad standing for the current UTC day, mirroring the
/// backend `RewardedAdStatusDto` (`GET /ads/rewarded/status`).
class RewardedAdStatus extends Equatable {
  const RewardedAdStatus({
    required this.grantsToday,
    required this.dailyLimit,
    required this.remainingToday,
    required this.diamondPerAd,
  });

  factory RewardedAdStatus.fromJson(Map<String, dynamic> json) {
    final grantsToday = json['grantsToday'];
    final dailyLimit = json['dailyLimit'];
    final remainingToday = json['remainingToday'];
    final diamondPerAd = json['diamondPerAd'];

    if (grantsToday is! int ||
        dailyLimit is! int ||
        remainingToday is! int ||
        diamondPerAd is! int) {
      throw StateError('Rewarded ad status response is missing fields.');
    }

    return RewardedAdStatus(
      grantsToday: grantsToday,
      dailyLimit: dailyLimit,
      remainingToday: remainingToday,
      diamondPerAd: diamondPerAd,
    );
  }

  final int grantsToday;
  final int dailyLimit;
  final int remainingToday;
  final int diamondPerAd;

  bool get isCapped => remainingToday <= 0;

  @override
  List<Object?> get props => [
    grantsToday,
    dailyLimit,
    remainingToday,
    diamondPerAd,
  ];
}
