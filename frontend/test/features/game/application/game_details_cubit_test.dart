import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/game/application/game_details_cubit.dart';
import 'package:lexilink_app/features/game/data/game_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

void main() {
  test('fills missing word descriptions from link details', () async {
    final requests = <String>[];
    final cubit = _createCubit((request) async {
      requests.add('${request.method} ${request.url.path}');

      return switch (request.url.path) {
        '/games/game-1' => http.Response(_gameJson(), 200),
        '/links/current-link' => http.Response(
          '{"description":"Current meaning"}',
          200,
        ),
        '/links/target-link' => http.Response(
          '{"description":"Target meaning"}',
          200,
        ),
        '/games/game-1/options' => http.Response('[]', 200),
        _ => http.Response('', 404),
      };
    });

    await cubit.loadGame('game-1');

    expect(cubit.state.status, GameDetailsStatus.success);
    expect(cubit.state.game?.currentDescription, 'Current meaning');
    expect(cubit.state.game?.targetDescription, 'Target meaning');
    expect(requests, contains('GET /links/current-link'));
    expect(requests, contains('GET /links/target-link'));

    await cubit.close();
  });

  test('keeps the game playable when fallback link details fail', () async {
    final cubit = _createCubit((request) async {
      return switch (request.url.path) {
        '/games/game-1' => http.Response(_gameJson(), 200),
        '/links/current-link' => http.Response('', 500),
        '/links/target-link' => http.Response('', 500),
        '/games/game-1/options' => http.Response('[]', 200),
        _ => http.Response('', 404),
      };
    });

    await cubit.loadGame('game-1');

    expect(cubit.state.status, GameDetailsStatus.success);
    expect(cubit.state.game?.currentDescription, isNull);
    expect(cubit.state.game?.targetDescription, isNull);

    await cubit.close();
  });
}

GameDetailsCubit _createCubit(
  Future<http.Response> Function(http.Request request) handler,
) {
  final tokenStore = InMemoryTokenStore();

  return GameDetailsCubit(
    gameRepository: GameRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: tokenStore,
        httpClient: MockClient(handler),
      ),
    ),
  );
}

String _gameJson() => '''
{
  "id": "game-1",
  "playerId": "player-1",
  "categoryId": "category-1",
  "difficulty": "Easy",
  "startLinkId": "start-link",
  "startWord": "Start",
  "targetLinkId": "target-link",
  "targetWord": "Target",
  "currentLinkId": "current-link",
  "currentWord": "Current",
  "state": "InProgress",
  "score": null,
  "maxSteps": 8,
  "stepsTaken": 0,
  "hintsTotal": 3,
  "hintsUsed": 0,
  "undosTotal": 5,
  "undosUsed": 0,
  "resetsTotal": 2,
  "resetsUsed": 0,
  "history": []
}
''';
