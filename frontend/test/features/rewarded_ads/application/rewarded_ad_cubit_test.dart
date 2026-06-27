import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/rewarded_ads/application/rewarded_ad_cubit.dart';
import 'package:lexilink_app/features/rewarded_ads/data/rewarded_ad_repository.dart';
import 'package:lexilink_app/shared/ads/ads_platform.dart';
import 'package:lexilink_app/shared/ads/ads_service.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

const _statusBody =
    '{"grantsToday":0,"dailyLimit":10,"remainingToday":10,"diamondPerAd":5}';
const _cappedBody =
    '{"grantsToday":10,"dailyLimit":10,"remainingToday":0,"diamondPerAd":5}';

class _FakeAdsPlatform implements AdsPlatform {
  _FakeAdsPlatform({required this.isSupported});

  @override
  final bool isSupported;

  int showRewardedCallCount = 0;
  String? lastUserId;
  bool failRewarded = false;

  @override
  Future<void> gatherConsent() async {}

  @override
  Future<void> initialize() async {}

  @override
  Future<void> showInterstitial(String adUnitId) async {}

  @override
  Future<void> showRewarded({
    required String adUnitId,
    required String userId,
    required void Function() onClosed,
    required void Function() onUnavailable,
  }) async {
    showRewardedCallCount++;
    lastUserId = userId;
    if (failRewarded) {
      onUnavailable();
    } else {
      onClosed();
    }
  }
}

void main() {
  group('RewardedAdCubit', () {
    blocTest<RewardedAdCubit, RewardedAdState>(
      'marks unavailable on an unsupported platform',
      build: () => _buildCubit(
        isSupported: false,
        handler: (_) async => http.Response(_statusBody, 200),
      ),
      act: (cubit) => cubit.load(),
      verify: (cubit) {
        expect(cubit.state.status, RewardedAdStatusState.unavailable);
      },
    );

    blocTest<RewardedAdCubit, RewardedAdState>(
      'loads the status when supported',
      build: () => _buildCubit(
        handler: (request) async {
          expect(request.url.path, '/ads/rewarded/status');
          return http.Response(_statusBody, 200);
        },
      ),
      act: (cubit) => cubit.load(),
      verify: (cubit) {
        expect(cubit.state.status, RewardedAdStatusState.ready);
        expect(cubit.state.data?.remainingToday, 10);
        expect(cubit.state.data?.diamondPerAd, 5);
      },
    );

    blocTest<RewardedAdCubit, RewardedAdState>(
      'watching shows the ad and flags reward-just-watched',
      build: () => _buildCubit(
        adsPlatform: _supportedPlatform,
        handler: (_) async => http.Response(_statusBody, 200),
      ),
      act: (cubit) async {
        await cubit.load();
        await cubit.watch();
      },
      verify: (cubit) {
        expect(_supportedPlatform.showRewardedCallCount, 1);
        expect(_supportedPlatform.lastUserId, 'player-1');
        expect(cubit.state.status, RewardedAdStatusState.ready);
        expect(cubit.state.rewardJustWatched, isTrue);
      },
    );

    blocTest<RewardedAdCubit, RewardedAdState>(
      'does not show an ad when the daily cap is reached',
      build: () => _buildCubit(
        adsPlatform: _cappedPlatform,
        handler: (_) async => http.Response(_cappedBody, 200),
      ),
      act: (cubit) async {
        await cubit.load();
        await cubit.watch();
      },
      verify: (cubit) {
        expect(_cappedPlatform.showRewardedCallCount, 0);
        expect(cubit.state.message, contains('limit'));
      },
    );

    blocTest<RewardedAdCubit, RewardedAdState>(
      'does not flag reward watched when the ad cannot load',
      build: () {
        final platform = _FakeAdsPlatform(isSupported: true)
          ..failRewarded = true;
        return _buildCubit(
          adsPlatform: platform,
          handler: (_) async => http.Response(_statusBody, 200),
        );
      },
      act: (cubit) async {
        await cubit.load();
        await cubit.watch();
      },
      verify: (cubit) {
        expect(cubit.state.status, RewardedAdStatusState.ready);
        expect(cubit.state.rewardJustWatched, isFalse);
        expect(cubit.state.message, contains('yüklenemedi'));
      },
    );
  });
}

final _supportedPlatform = _FakeAdsPlatform(isSupported: true);
final _cappedPlatform = _FakeAdsPlatform(isSupported: true);

RewardedAdCubit _buildCubit({
  required MockClientHandler handler,
  bool isSupported = true,
  _FakeAdsPlatform? adsPlatform,
}) {
  final platform = adsPlatform ?? _FakeAdsPlatform(isSupported: isSupported);
  final adsService = AdsService(platform: platform)..initialize();

  return RewardedAdCubit(
    repository: RewardedAdRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient(handler),
      ),
    ),
    adsService: adsService,
    userId: 'player-1',
    isSupported: platform.isSupported,
  );
}
