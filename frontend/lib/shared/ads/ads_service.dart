import 'dart:math';

import 'package:lexilink_app/shared/ads/ad_config.dart';
import 'package:lexilink_app/shared/ads/ads_platform.dart';
import 'package:lexilink_app/shared/ads/mobile_ads_platform.dart';

/// App-wide ads facade, provided above the router like `AudioService` — ads
/// are a genuinely global concern. Mobile-only: on web/desktop it reports
/// [isSupported] `false` and every operation is a safe no-op, so callers
/// never branch on platform. Initialization and playback are best-effort; a
/// failed or slow ad network must never block app start or gameplay.
class AdsService {
  AdsService({AdsPlatform? platform, Random? random})
    : _platform = platform ?? const MobileAdsPlatform(),
      _random = random ?? Random();

  final AdsPlatform _platform;
  final Random _random;
  bool _initialized = false;

  /// Whether ads can run on this platform (`false` on web/desktop).
  bool get isSupported => _platform.isSupported;

  /// Whether the ad SDK has finished initializing.
  bool get isInitialized => _initialized;

  /// Gathers AdMob UMP consent and then initializes the ad SDK,
  /// once, on supported platforms; no-op otherwise. Consent is gathered
  /// **before** the SDK is initialized so it precedes any ad request.
  /// Swallows failures so app start never depends on the ad network.
  Future<void> initialize() async {
    if (_initialized || !_platform.isSupported) return;
    try {
      await _platform.gatherConsent();
      await _platform.initialize();
      _initialized = true;
    } on Object catch (_) {
      // Best-effort: ads must never block app startup.
    }
  }

  /// Shows an interstitial with the given [probability] (0..1). No-op when
  /// ads are unsupported, the SDK isn't initialized, or the dice roll misses.
  /// Fire-and-forget and best-effort — never throws into the UI, never blocks
  /// the surrounding navigation/flow.
  Future<void> maybeShowInterstitial(double probability) async {
    if (!_platform.isSupported || !_initialized) return;
    if (_random.nextDouble() >= probability) return;
    final adUnitId = AdConfig.interstitialAdUnitId;
    if (adUnitId.isEmpty) return;

    try {
      await _platform.showInterstitial(adUnitId);
    } on Object catch (_) {
      // Best-effort: a failed interstitial never interrupts gameplay.
    }
  }

  /// Shows a rewarded ad tagged with [userId] as the SSV `user_id`. The
  /// Diamond grant is backend-owned (verified SSV callback), never the local
  /// earned-reward event. [onClosed] is invoked only after an ad was shown and
  /// dismissed. [onUnavailable] is invoked when the SDK is unsupported, not
  /// initialized, or the ad cannot be loaded/shown.
  Future<void> showRewarded({
    required String userId,
    required void Function() onClosed,
    required void Function() onUnavailable,
  }) async {
    if (!_platform.isSupported || !_initialized) {
      onUnavailable();
      return;
    }
    final adUnitId = AdConfig.rewardedAdUnitId;
    if (adUnitId.isEmpty) {
      onUnavailable();
      return;
    }

    try {
      await _platform.showRewarded(
        adUnitId: adUnitId,
        userId: userId,
        onClosed: onClosed,
        onUnavailable: onUnavailable,
      );
    } on Object catch (_) {
      onUnavailable();
    }
  }
}
