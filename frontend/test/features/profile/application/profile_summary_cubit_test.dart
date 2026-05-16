import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/profile/application/profile_summary_cubit.dart';
import 'package:lexilink_app/features/profile/data/player_stats_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

const _playerStatsBody = '''
{
  "playerId": "player-1",
  "displayName": "Yasin",
  "discriminator": 1234,
  "handle": "Yasin#1234",
  "avatarUrl": null,
  "locale": "tr-TR",
  "isGuest": true,
  "authProvidersLinked": 0,
  "gamesCompleted": 2,
  "bestScore": 300,
  "totalScore": 500,
  "lastGameCompletedOn": "2026-05-13T10:00:00Z",
  "createdAt": "2026-05-13T09:00:00Z",
  "updatedAt": "2026-05-13T10:00:00Z"
}
''';

ProfileSummaryCubit _buildCubit({
  required TokenStore tokenStore,
  required MockClientHandler handler,
}) {
  final apiClient = ApiClient(
    config: const ApiConfig(baseUrl: 'http://localhost:5000'),
    tokenStore: tokenStore,
    httpClient: MockClient(handler),
  );

  return ProfileSummaryCubit(
    playerStatsRepository: PlayerStatsRepository(apiClient: apiClient),
    tokenStore: tokenStore,
  );
}

void main() {
  group('ProfileSummaryCubit', () {
    blocTest<ProfileSummaryCubit, ProfileSummaryState>(
      'loads player stats from session player id',
      build: () {
        final tokenStore = InMemoryTokenStore()..saveAccessToken('player-1');

        return _buildCubit(
          tokenStore: tokenStore,
          handler: (request) async {
            expect(request.url.path, '/stats/players/player-1');

            return http.Response(_playerStatsBody, 200);
          },
        );
      },
      act: (cubit) => cubit.loadSummary(),
      verify: (cubit) {
        expect(cubit.state.status, ProfileSummaryStatus.success);
        expect(cubit.state.stats?.handle, 'Yasin#1234');
        expect(cubit.state.stats?.gamesCompleted, 2);
        expect(cubit.state.stats?.bestScore, 300);
      },
    );

    blocTest<ProfileSummaryCubit, ProfileSummaryState>(
      'fails when session player id is missing',
      build: () => _buildCubit(
        tokenStore: InMemoryTokenStore(),
        handler: (_) async => http.Response('', 500),
      ),
      act: (cubit) => cubit.loadSummary(),
      expect: () => const [
        ProfileSummaryState.loading(),
        ProfileSummaryState.failure(message: 'Guest session is missing.'),
      ],
    );

    blocTest<ProfileSummaryCubit, ProfileSummaryState>(
      'maps ApiException to failure message',
      build: () {
        final tokenStore = InMemoryTokenStore()..saveAccessToken('player-1');

        return _buildCubit(
          tokenStore: tokenStore,
          handler: (_) async => http.Response('', 401),
        );
      },
      act: (cubit) => cubit.loadSummary(),
      expect: () => const [
        ProfileSummaryState.loading(),
        ProfileSummaryState.failure(message: 'Authentication is required.'),
      ],
    );
  });
}
