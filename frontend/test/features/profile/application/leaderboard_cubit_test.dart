import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/profile/application/leaderboard_cubit.dart';
import 'package:lexilink_app/features/profile/data/leaderboard_query.dart';
import 'package:lexilink_app/features/profile/data/player_stats_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

const _leaderboardBody = '''
[
  {
    "playerId": "player-1",
    "displayName": "Yasin",
    "discriminator": 1234,
    "handle": "Yasin#1234",
    "avatarUrl": null,
    "locale": "tr-TR",
    "gamesCompleted": 5,
    "bestScore": 800,
    "totalScore": 2400,
    "lastGameCompletedOn": "2026-05-13T10:00:00Z"
  },
  {
    "playerId": "player-2",
    "displayName": "Ada",
    "discriminator": 42,
    "handle": "Ada#0042",
    "avatarUrl": null,
    "locale": "tr-TR",
    "gamesCompleted": 3,
    "bestScore": 600,
    "totalScore": 1200,
    "lastGameCompletedOn": "2026-05-12T10:00:00Z"
  }
]
''';

LeaderboardCubit _buildCubit({required MockClientHandler handler}) {
  final apiClient = ApiClient(
    config: const ApiConfig(baseUrl: 'http://localhost:5000'),
    tokenStore: InMemoryTokenStore(),
    httpClient: MockClient(handler),
  );

  return LeaderboardCubit(
    playerStatsRepository: PlayerStatsRepository(apiClient: apiClient),
  );
}

void main() {
  group('LeaderboardCubit', () {
    blocTest<LeaderboardCubit, LeaderboardState>(
      'loads leaderboard entries in backend order',
      build: () => _buildCubit(
        handler: (request) async {
          expect(request.url.path, '/stats/leaderboard');
          expect(request.url.queryParameters['orderBy'], 'totalScore');
          expect(request.url.queryParameters['period'], 'allTime');

          return http.Response(_leaderboardBody, 200);
        },
      ),
      act: (cubit) => cubit.loadLeaderboard(),
      verify: (cubit) {
        expect(cubit.state.status, LeaderboardStatus.success);
        expect(cubit.state.entries, hasLength(2));
        expect(cubit.state.entries.first.handle, 'Yasin#1234');
        expect(cubit.state.entries.first.bestScore, 800);
        expect(cubit.state.entries.last.handle, 'Ada#0042');
      },
    );

    blocTest<LeaderboardCubit, LeaderboardState>(
      'emits success with empty list when backend returns no entries',
      build: () => _buildCubit(
        handler: (_) async => http.Response('[]', 200),
      ),
      act: (cubit) => cubit.loadLeaderboard(),
      verify: (cubit) {
        expect(cubit.state.status, LeaderboardStatus.success);
        expect(cubit.state.entries, isEmpty);
      },
    );

    blocTest<LeaderboardCubit, LeaderboardState>(
      'maps ApiException to failure message',
      build: () => _buildCubit(
        handler: (_) async => http.Response('', 401),
      ),
      act: (cubit) => cubit.loadLeaderboard(),
      expect: () => const [
        LeaderboardState.loading(),
        LeaderboardState.failure(message: 'Authentication is required.'),
      ],
    );

    blocTest<LeaderboardCubit, LeaderboardState>(
      'changePeriod reloads with selected period',
      build: () {
        final receivedPeriods = <String?>[];
        return _buildCubit(
          handler: (request) async {
            receivedPeriods.add(request.url.queryParameters['period']);
            return http.Response(_leaderboardBody, 200);
          },
        );
      },
      act: (cubit) async {
        await cubit.loadLeaderboard();
        await cubit.changePeriod(LeaderboardPeriod.weekly);
      },
      verify: (cubit) {
        expect(cubit.state.status, LeaderboardStatus.success);
        expect(cubit.state.query.period, LeaderboardPeriod.weekly);
      },
    );

    blocTest<LeaderboardCubit, LeaderboardState>(
      'changePeriod skips when same period is already loaded',
      build: () => _buildCubit(
        handler: (_) async => http.Response(_leaderboardBody, 200),
      ),
      act: (cubit) async {
        await cubit.loadLeaderboard();
        await cubit.changePeriod(LeaderboardPeriod.allTime);
      },
      expect: () => [
        const LeaderboardState.loading(),
        isA<LeaderboardState>().having(
          (s) => s.status,
          'status',
          LeaderboardStatus.success,
        ),
      ],
    );
  });
}
