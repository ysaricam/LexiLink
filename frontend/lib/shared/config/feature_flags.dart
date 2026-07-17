class FeatureFlags {
  const FeatureFlags._();

  static const bool adsEnabled = bool.fromEnvironment(
    'LEXILINK_ENABLE_ADS',
  );

  static const bool rewardedAdsEnabled =
      bool.fromEnvironment(
        'LEXILINK_ENABLE_REWARDED_ADS',
      ) &&
      adsEnabled;

  static const bool iapEnabled = bool.fromEnvironment(
    'LEXILINK_ENABLE_IAP',
  );
}
