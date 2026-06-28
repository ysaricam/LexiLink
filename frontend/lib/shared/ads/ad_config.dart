import 'package:flutter/foundation.dart';

/// AdMob ad-unit identifiers.
///
/// Defaults are Google's official **test** ad-unit ids so ads work locally
/// without a real AdMob account; real ids drop in via `--dart-define` with
/// no code change — the same "placeholder now, real later" shape as the
/// audio assets. The AdMob **app** ids live in `AndroidManifest.xml` /
/// `Info.plist` (also Google test ids until real credentials arrive).
class AdConfig {
  const AdConfig._();

  // Google's public test ad-unit ids — always fill, never bill.
  static const String _testInterstitialAndroid =
      'ca-app-pub-3940256099942544/1033173712';
  static const String _testInterstitialIos =
      'ca-app-pub-3940256099942544/4411468910';
  static const String _testRewardedAndroid =
      'ca-app-pub-3940256099942544/5224354917';
  static const String _testRewardedIos =
      'ca-app-pub-3940256099942544/1712485313';

  /// Interstitial ad-unit id for the current platform.
  /// Override with `--dart-define=ADMOB_INTERSTITIAL_AD_UNIT_ID=...`.
  static String get interstitialAdUnitId {
    const override = String.fromEnvironment('ADMOB_INTERSTITIAL_AD_UNIT_ID');
    if (override.isNotEmpty) return override;
    if (kReleaseMode) return '';
    return _isIos ? _testInterstitialIos : _testInterstitialAndroid;
  }

  /// Rewarded ad-unit id for the current platform.
  /// Override with `--dart-define=ADMOB_REWARDED_AD_UNIT_ID=...`.
  static String get rewardedAdUnitId {
    const override = String.fromEnvironment('ADMOB_REWARDED_AD_UNIT_ID');
    if (override.isNotEmpty) return override;
    if (kReleaseMode) return '';
    return _isIos ? _testRewardedIos : _testRewardedAndroid;
  }

  static bool get _isIos =>
      !kIsWeb && defaultTargetPlatform == TargetPlatform.iOS;
}

/// Probability that an interstitial is shown at each placement. Frontend
/// constants, easily tunable; not admin-configurable in v1.
class InterstitialChance {
  const InterstitialChance._();

  /// ~1 in 3 game starts.
  static const double gameStart = 1 / 3;

  /// ~1 in 2 game finishes.
  static const double gameEnd = 1 / 2;
}
