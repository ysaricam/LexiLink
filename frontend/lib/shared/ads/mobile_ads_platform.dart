import 'dart:async';

import 'package:app_tracking_transparency/app_tracking_transparency.dart';
import 'package:flutter/foundation.dart';
import 'package:google_mobile_ads/google_mobile_ads.dart';
import 'package:lexilink_app/shared/ads/ads_platform.dart';

/// Production [AdsPlatform] backed by the `google_mobile_ads` SDK. This is
/// the only file in the app that imports the SDK; everything else talks to
/// the [AdsPlatform] abstraction. Ads run on Android/iOS only — web and
/// desktop report unsupported so `AdsService` no-ops there.
class MobileAdsPlatform implements AdsPlatform {
  const MobileAdsPlatform();

  @override
  bool get isSupported =>
      !kIsWeb &&
      (defaultTargetPlatform == TargetPlatform.android ||
          defaultTargetPlatform == TargetPlatform.iOS);

  @override
  Future<void> gatherConsent() async {
    // iOS App Tracking Transparency: prompt only when the user hasn't decided
    // yet. Best-effort; declining is fine — ads still serve (non-personalized).
    if (defaultTargetPlatform == TargetPlatform.iOS) {
      try {
        final status =
            await AppTrackingTransparency.trackingAuthorizationStatus;
        if (status == TrackingStatus.notDetermined) {
          await AppTrackingTransparency.requestTrackingAuthorization();
        }
      } on Object catch (_) {
        // Best-effort: never block ads/app start on the ATT prompt.
      }
    }

    // AdMob UMP consent: refresh consent info, then show the form if required.
    try {
      await _requestUmpConsent();
    } on Object catch (_) {
      // Best-effort.
    }
  }

  Future<void> _requestUmpConsent() {
    final completer = Completer<void>();
    void complete() {
      if (!completer.isCompleted) completer.complete();
    }

    ConsentInformation.instance.requestConsentInfoUpdate(
      ConsentRequestParameters(),
      () async {
        try {
          await ConsentForm.loadAndShowConsentFormIfRequired((_) {});
        } on Object catch (_) {
          // Best-effort.
        } finally {
          complete();
        }
      },
      (_) => complete(),
    );

    return completer.future;
  }

  @override
  Future<void> initialize() => MobileAds.instance.initialize();

  @override
  Future<void> showInterstitial(String adUnitId) {
    return InterstitialAd.load(
      adUnitId: adUnitId,
      request: const AdRequest(),
      adLoadCallback: InterstitialAdLoadCallback(
        onAdLoaded: (ad) {
          ad
            ..fullScreenContentCallback = FullScreenContentCallback(
              onAdDismissedFullScreenContent: (ad) => ad.dispose(),
              onAdFailedToShowFullScreenContent: (ad, _) => ad.dispose(),
            )
            ..show();
        },
        // Best-effort: a failed load is silently ignored — interstitials
        // never block gameplay or navigation.
        onAdFailedToLoad: (error) {
          if (kDebugMode) {
            debugPrint('AdMob interstitial failed to load: $error');
          }
        },
      ),
    );
  }

  @override
  Future<void> showRewarded({
    required String adUnitId,
    required String userId,
    required void Function() onClosed,
    required void Function() onUnavailable,
  }) {
    if (kDebugMode) {
      debugPrint('AdMob rewarded loading adUnitId: $adUnitId');
    }

    return RewardedAd.load(
      adUnitId: adUnitId,
      request: const AdRequest(),
      rewardedAdLoadCallback: RewardedAdLoadCallback(
        onAdLoaded: (ad) {
          ad
            ..setServerSideOptions(
              ServerSideVerificationOptions(userId: userId),
            )
            ..fullScreenContentCallback = FullScreenContentCallback(
              onAdDismissedFullScreenContent: (ad) {
                ad.dispose();
                onClosed();
              },
              onAdFailedToShowFullScreenContent: (ad, error) {
                if (kDebugMode) {
                  debugPrint('AdMob rewarded failed to show: $error');
                }
                ad.dispose();
                onUnavailable();
              },
            )
            // The Diamond grant is backend-owned via SSV; the local
            // earned-reward callback is intentionally a no-op.
            ..show(onUserEarnedReward: (_, _) {});
        },
        onAdFailedToLoad: (error) {
          if (kDebugMode) {
            debugPrint('AdMob rewarded failed to load: $error');
          }
          onUnavailable();
        },
      ),
    );
  }
}
