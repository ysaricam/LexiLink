import 'dart:math';

import 'package:flutter_test/flutter_test.dart';
import 'package:lexilink_app/shared/ads/ad_config.dart';
import 'package:lexilink_app/shared/ads/ads_platform.dart';
import 'package:lexilink_app/shared/ads/ads_service.dart';

class _FakeAdsPlatform implements AdsPlatform {
  _FakeAdsPlatform({required this.isSupported, this.throwOnInit = false});

  @override
  final bool isSupported;

  final bool throwOnInit;
  int initializeCallCount = 0;
  int gatherConsentCallCount = 0;
  int showInterstitialCallCount = 0;
  int showRewardedCallCount = 0;
  String? lastRewardedUserId;
  bool autoInvokeOnClosed = true;
  bool autoInvokeOnUnavailable = false;
  final List<String> callLog = [];

  @override
  Future<void> gatherConsent() async {
    gatherConsentCallCount++;
    callLog.add('consent');
  }

  @override
  Future<void> initialize() async {
    initializeCallCount++;
    callLog.add('init');
    if (throwOnInit) {
      throw StateError('ad sdk init failed');
    }
  }

  @override
  Future<void> showInterstitial(String adUnitId) async {
    showInterstitialCallCount++;
  }

  @override
  Future<void> showRewarded({
    required String adUnitId,
    required String userId,
    required void Function() onClosed,
    required void Function() onUnavailable,
  }) async {
    showRewardedCallCount++;
    lastRewardedUserId = userId;
    if (autoInvokeOnClosed) onClosed();
    if (autoInvokeOnUnavailable) onUnavailable();
  }
}

/// A [Random] returning a fixed sequence so the probability gate is
/// deterministic in tests.
class _ScriptedRandom implements Random {
  _ScriptedRandom(this._values);

  final List<double> _values;
  int _cursor = 0;

  @override
  double nextDouble() => _values[_cursor++ % _values.length];

  @override
  bool nextBool() => throw UnimplementedError();

  @override
  int nextInt(int max) => throw UnimplementedError();
}

void main() {
  group('AdsService', () {
    test('initializes the SDK once on a supported platform', () async {
      final platform = _FakeAdsPlatform(isSupported: true);
      final service = AdsService(platform: platform);

      await service.initialize();

      expect(service.isSupported, isTrue);
      expect(service.isInitialized, isTrue);
      expect(platform.initializeCallCount, 1);
    });

    test('is idempotent — repeated initialize calls the SDK once', () async {
      final platform = _FakeAdsPlatform(isSupported: true);
      final service = AdsService(platform: platform);

      await service.initialize();
      await service.initialize();

      expect(platform.initializeCallCount, 1);
    });

    test('gathers consent before initializing the SDK', () async {
      final platform = _FakeAdsPlatform(isSupported: true);
      final service = AdsService(platform: platform);

      await service.initialize();

      expect(platform.callLog, ['consent', 'init']);
    });

    test('does not gather consent on an unsupported platform', () async {
      final platform = _FakeAdsPlatform(isSupported: false);
      final service = AdsService(platform: platform);

      await service.initialize();

      expect(platform.gatherConsentCallCount, 0);
      expect(platform.callLog, isEmpty);
    });

    test('no-ops on an unsupported platform (web/desktop)', () async {
      final platform = _FakeAdsPlatform(isSupported: false);
      final service = AdsService(platform: platform);

      await service.initialize();

      expect(service.isSupported, isFalse);
      expect(service.isInitialized, isFalse);
      expect(platform.initializeCallCount, 0);
    });

    test('swallows SDK init failures so startup is never blocked', () async {
      final platform = _FakeAdsPlatform(isSupported: true, throwOnInit: true);
      final service = AdsService(platform: platform);

      await service.initialize();

      expect(service.isInitialized, isFalse);
      expect(platform.initializeCallCount, 1);
    });
  });

  group('AdsService.maybeShowInterstitial', () {
    test('shows when the dice roll is under the probability', () async {
      final platform = _FakeAdsPlatform(isSupported: true);
      final service = AdsService(
        platform: platform,
        random: _ScriptedRandom([0.1]),
      );
      await service.initialize();

      await service.maybeShowInterstitial(InterstitialChance.gameStart);

      expect(platform.showInterstitialCallCount, 1);
    });

    test('skips when the dice roll is at/above the probability', () async {
      final platform = _FakeAdsPlatform(isSupported: true);
      final service = AdsService(
        platform: platform,
        random: _ScriptedRandom([0.9]),
      );
      await service.initialize();

      await service.maybeShowInterstitial(InterstitialChance.gameStart);

      expect(platform.showInterstitialCallCount, 0);
    });

    test('no-ops when not initialized even on a winning roll', () async {
      final platform = _FakeAdsPlatform(isSupported: true);
      final service = AdsService(
        platform: platform,
        random: _ScriptedRandom([0.0]),
      );

      await service.maybeShowInterstitial(InterstitialChance.gameEnd);

      expect(platform.showInterstitialCallCount, 0);
    });

    test(
      'keeps review-build interstitials disabled without initialization',
      () async {
        final platform = _FakeAdsPlatform(isSupported: true);
        final service = AdsService(
          platform: platform,
          random: _ScriptedRandom([0.0]),
        );

        await service.maybeShowInterstitial(1);

        expect(service.isInitialized, isFalse);
        expect(platform.initializeCallCount, 0);
        expect(platform.showInterstitialCallCount, 0);
      },
    );

    test('no-ops on an unsupported platform', () async {
      final platform = _FakeAdsPlatform(isSupported: false);
      final service = AdsService(
        platform: platform,
        random: _ScriptedRandom([0.0]),
      );
      await service.initialize();

      await service.maybeShowInterstitial(InterstitialChance.gameStart);

      expect(platform.showInterstitialCallCount, 0);
    });
  });

  group('AdsService.showRewarded', () {
    test('forwards the user id and signals close when supported', () async {
      final platform = _FakeAdsPlatform(isSupported: true);
      final service = AdsService(platform: platform);
      await service.initialize();
      var closed = false;

      await service.showRewarded(
        userId: 'player-1',
        onClosed: () => closed = true,
        onUnavailable: () {},
      );

      expect(platform.showRewardedCallCount, 1);
      expect(platform.lastRewardedUserId, 'player-1');
      expect(closed, isTrue);
    });

    test('signals unavailable when unsupported (so the UI recovers)', () async {
      final platform = _FakeAdsPlatform(isSupported: false);
      final service = AdsService(platform: platform);
      await service.initialize();
      var closed = false;
      var unavailable = false;

      await service.showRewarded(
        userId: 'player-1',
        onClosed: () => closed = true,
        onUnavailable: () => unavailable = true,
      );

      expect(platform.showRewardedCallCount, 0);
      expect(closed, isFalse);
      expect(unavailable, isTrue);
    });

    test('signals unavailable when not initialized', () async {
      final platform = _FakeAdsPlatform(isSupported: true);
      final service = AdsService(platform: platform);
      var closed = false;
      var unavailable = false;

      await service.showRewarded(
        userId: 'player-1',
        onClosed: () => closed = true,
        onUnavailable: () => unavailable = true,
      );

      expect(platform.showRewardedCallCount, 0);
      expect(closed, isFalse);
      expect(unavailable, isTrue);
    });
  });

  group('AdConfig', () {
    test('exposes non-empty test interstitial and rewarded ad-unit ids', () {
      expect(AdConfig.interstitialAdUnitId, isNotEmpty);
      expect(AdConfig.rewardedAdUnitId, isNotEmpty);
      expect(AdConfig.interstitialAdUnitId, startsWith('ca-app-pub-'));
      expect(AdConfig.rewardedAdUnitId, startsWith('ca-app-pub-'));
    });

    test('interstitial chances are sane probabilities', () {
      expect(InterstitialChance.gameStart, closeTo(1 / 3, 1e-9));
      expect(InterstitialChance.gameEnd, closeTo(1 / 2, 1e-9));
    });
  });
}
