/// Seam over the `google_mobile_ads` SDK so `AdsService` stays unit-testable
/// and the rest of the app never imports the SDK directly. The production
/// implementation is `MobileAdsPlatform`; tests inject a fake.
abstract class AdsPlatform {
  /// Whether ads can run on the current platform. `false` on web and desktop;
  /// `true` only on Android/iOS.
  bool get isSupported;

  /// Gathers user consent before any ad request: the iOS App Tracking
  /// Transparency prompt (iOS only) followed by the AdMob UMP consent flow.
  /// Best-effort — a failure or a user declining must never block ads or app
  /// start. Called by `AdsService.initialize` before [initialize].
  Future<void> gatherConsent();

  /// Initializes the underlying ad SDK. Called once at app start.
  Future<void> initialize();

  /// Loads and shows a single interstitial ad for [adUnitId]. Fire-and-forget
  /// and best-effort: a load/show failure must never surface to the caller.
  Future<void> showInterstitial(String adUnitId);

  /// Loads and shows a single rewarded ad for [adUnitId], tagging the request
  /// with [userId] as the AdMob Server-Side Verification `user_id` so the
  /// backend can resolve the player when AdMob's signed callback arrives.
  /// The Diamond grant is backend-owned — the local earned-reward callback is
  /// intentionally ignored. [onClosed] is invoked exactly once when the ad is
  /// dismissed, fails to show, or fails to load, so the caller can recover and
  /// refresh state.
  Future<void> showRewarded({
    required String adUnitId,
    required String userId,
    required void Function() onClosed,
  });
}
