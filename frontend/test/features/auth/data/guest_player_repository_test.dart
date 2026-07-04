import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/auth/data/guest_player_repository.dart';
import 'package:lexilink_app/features/auth/data/social_identity.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

void main() {
  test('registers guest then exchanges identity for an access token', () async {
    final calls = <String>[];
    final repository = GuestPlayerRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient((request) async {
          calls.add('${request.method} ${request.url.path}');
          if (request.url.path == '/players/guest') {
            expect(request.body, contains('"deviceId":"device-1"'));
            expect(request.body, contains('"displayName":"Guest Player"'));
            expect(request.body, contains('"locale":"en-US"'));
            return http.Response('{"id":"player-1"}', 201);
          }
          if (request.url.path == '/auth/token') {
            expect(request.body, contains('"provider":"Guest"'));
            expect(request.body, contains('"externalId":"device-1"'));
            expect(
              request.body,
              contains('"externalToken":"dev:Guest:device-1"'),
            );
            return http.Response(
              '{'
              '"accessToken":"jwt-player-1",'
              '"expiresAt":"2026-05-23T12:00:00Z",'
              '"playerId":"player-1"'
              '}',
              200,
            );
          }
          fail('Unexpected request: ${request.method} ${request.url.path}');
        }),
      ),
    );

    final session = await repository.registerGuest(
      deviceId: 'device-1',
      displayName: 'Guest Player',
      locale: 'en-US',
    );

    expect(session.playerId, 'player-1');
    expect(session.accessToken, 'jwt-player-1');
    expect(calls, ['POST /players/guest', 'POST /auth/token']);
  });

  test('exchanges Apple identity for an access token', () async {
    final repository = GuestPlayerRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient((request) async {
          expect(request.method, 'POST');
          expect(request.url.path, '/auth/token');
          expect(request.body, contains('"provider":"Apple"'));
          expect(request.body, contains('"externalId":"apple-user-1"'));
          expect(request.body, contains('"externalToken":"apple-token-1"'));
          return http.Response(
            '{'
            '"accessToken":"jwt-apple-player",'
            '"expiresAt":"2026-05-23T12:00:00Z",'
            '"playerId":"apple-player-1"'
            '}',
            200,
          );
        }),
      ),
    );

    final session = await repository.exchangeSocialIdentity(
      const SocialIdentity(
        provider: SocialAuthProvider.apple,
        externalId: 'apple-user-1',
        externalToken: 'apple-token-1',
      ),
    );

    expect(session.playerId, 'apple-player-1');
    expect(session.accessToken, 'jwt-apple-player');
  });
}
