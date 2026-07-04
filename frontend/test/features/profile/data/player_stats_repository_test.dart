import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/auth/data/social_identity.dart';
import 'package:lexilink_app/features/profile/data/account_link_repository.dart';
import 'package:lexilink_app/features/profile/data/leaderboard_query.dart';
import 'package:lexilink_app/features/profile/data/player_stats_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

void main() {
  test('gets player stats', () async {
    final repository = PlayerStatsRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient((request) async {
          expect(request.url.path, '/stats/players/player-1');

          return http.Response(
            '''
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
''',
            200,
          );
        }),
      ),
    );

    final stats = await repository.getPlayerStats('player-1');

    expect(stats.playerId, 'player-1');
    expect(stats.handle, 'Yasin#1234');
    expect(stats.gamesCompleted, 2);
    expect(stats.bestScore, 300);
    expect(stats.lastGameCompletedOn, DateTime.parse('2026-05-13T10:00:00Z'));
  });

  test('gets leaderboard with query parameters', () async {
    final repository = PlayerStatsRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient((request) async {
          expect(request.url.path, '/stats/leaderboard');
          expect(request.url.queryParameters['orderBy'], 'totalScore');
          expect(request.url.queryParameters['period'], 'weekly');
          expect(request.url.queryParameters['periodStart'], '2026-05-11');
          expect(request.url.queryParameters['limit'], '10');

          return http.Response(
            '''
[
  {
    "playerId": "player-1",
    "displayName": "Yasin",
    "discriminator": 1234,
    "handle": "Yasin#1234",
    "avatarUrl": null,
    "locale": "tr-TR",
    "gamesCompleted": 2,
    "bestScore": 300,
    "totalScore": 500,
    "lastGameCompletedOn": "2026-05-13T10:00:00Z"
  }
]
''',
            200,
          );
        }),
      ),
    );

    final entries = await repository.getLeaderboard(
      query: LeaderboardQuery(
        period: LeaderboardPeriod.weekly,
        periodStart: DateTime(2026, 5, 11),
        limit: 10,
      ),
    );

    expect(entries, hasLength(1));
    expect(entries.single.playerId, 'player-1');
    expect(entries.single.totalScore, 500);
  });

  test('continues with Apple and parses returned session', () async {
    final repository = AccountLinkRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient((request) async {
          expect(request.url.path, '/auth/apple/continue');
          expect(request.method, 'POST');
          expect(request.body, contains('"externalId":"apple-sub"'));
          expect(request.body, contains('"externalToken":"id-token"'));
          expect(request.body, contains('"email":"yasin@example.com"'));

          return http.Response(
            '{'
            '"accessToken":"jwt-apple",'
            '"expiresAt":"2026-05-23T12:00:00Z",'
            '"playerId":"apple-player-1",'
            '"mode":"SwitchedToExistingApplePlayer"'
            '}',
            200,
          );
        }),
      ),
    );

    final session = await repository.continueWithApple(
      identity: const SocialIdentity(
        provider: SocialAuthProvider.apple,
        externalId: 'apple-sub',
        externalToken: 'id-token',
        email: 'yasin@example.com',
      ),
    );

    expect(session.accessToken, 'jwt-apple');
    expect(session.playerId, 'apple-player-1');
    expect(session.mode, AppleContinueMode.switchedToExistingApplePlayer);
  });
}
