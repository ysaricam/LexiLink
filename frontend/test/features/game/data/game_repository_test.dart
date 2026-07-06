import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/game/data/game_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

void main() {
  test('creates and starts game', () async {
    final requests = <String>[];
    final repository = GameRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient((request) async {
          requests.add('${request.method} ${request.url.path}');

          if (request.url.path == '/games') {
            expect(request.body, contains('"playerId":"player-1"'));
            expect(request.body, contains('"categoryId":"category-1"'));
            expect(request.body, contains('"difficulty":"Easy"'));

            return http.Response('{"id":"game-1"}', 201);
          }

          return http.Response('', 204);
        }),
      ),
    );

    final gameId = await repository.createGame(
      playerId: 'player-1',
      categoryId: 'category-1',
    );
    await repository.startGame(gameId);

    expect(gameId, 'game-1');
    expect(requests, ['POST /games', 'POST /games/game-1/start']);
  });

  test('gets game options', () async {
    final repository = GameRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient((request) async {
          expect(request.url.path, '/games/game-1/options');

          return http.Response(
            '[{"id":"link-2","value":"basketbol","isActive":true}]',
            200,
          );
        }),
      ),
    );

    final options = await repository.getOptions('game-1');

    expect(options, hasLength(1));
    expect(options.single.id, 'link-2');
    expect(options.single.value, 'basketbol');
    expect(options.single.isActive, isTrue);
  });

  test('gets link description', () async {
    final repository = GameRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient((request) async {
          expect(request.url.path, '/links/link-1');

          return http.Response(
            '{"id":"link-1","categoryId":"category-1","value":"Spor",'
            '"description":"Bedeni gelistiren etkinlik.","isActive":true}',
            200,
          );
        }),
      ),
    );

    final description = await repository.getLinkDescription('link-1');

    expect(description, 'Bedeni gelistiren etkinlik.');
  });

  test('makes step', () async {
    final repository = GameRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient((request) async {
          expect(request.method, 'POST');
          expect(request.url.path, '/games/game-1/steps');
          expect(request.body, contains('"nextLinkId":"link-2"'));

          return http.Response('', 204);
        }),
      ),
    );

    await repository.makeStep(gameId: 'game-1', nextLinkId: 'link-2');
  });

  test('uses hint', () async {
    final repository = GameRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient((request) async {
          expect(request.method, 'POST');
          expect(request.url.path, '/games/game-1/hint');

          return http.Response(
            '{"type":"OnCorrectPath","recommendedLinkId":"link-2"}',
            200,
          );
        }),
      ),
    );

    final hint = await repository.useHint('game-1');

    expect(hint.type, 'OnCorrectPath');
    expect(hint.recommendedLinkId, 'link-2');
  });

  test('runs game control actions', () async {
    final requests = <String>[];
    final repository = GameRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient((request) async {
          requests.add('${request.method} ${request.url.path}');

          return http.Response('', 204);
        }),
      ),
    );

    await repository.undo('game-1');
    await repository.reset('game-1');
    await repository.abandon('game-1');

    expect(requests, [
      'POST /games/game-1/undo',
      'POST /games/game-1/reset',
      'POST /games/game-1/abandon',
    ]);
  });
}
