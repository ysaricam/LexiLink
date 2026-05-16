import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/auth/data/guest_player_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

void main() {
  test('posts guest registration request', () async {
    final repository = GuestPlayerRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient((request) async {
          expect(request.url.path, '/players/guest');
          expect(request.body, contains('"deviceId":"device-1"'));
          expect(request.body, contains('"displayName":"Guest Player"'));
          expect(request.body, contains('"locale":"en-US"'));

          return http.Response('{"id":"player-1"}', 201);
        }),
      ),
    );

    final playerId = await repository.registerGuest(
      deviceId: 'device-1',
      displayName: 'Guest Player',
      locale: 'en-US',
    );

    expect(playerId, 'player-1');
  });
}
