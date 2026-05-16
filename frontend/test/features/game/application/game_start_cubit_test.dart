import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/game/application/game_start_cubit.dart';
import 'package:lexilink_app/features/game/data/game_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

void main() {
  group('GameStartCubit', () {
    blocTest<GameStartCubit, GameStartState>(
      'creates and starts game',
      build: () {
        final tokenStore = InMemoryTokenStore()..saveAccessToken('player-1');

        return GameStartCubit(
          tokenStore: tokenStore,
          gameRepository: GameRepository(
            apiClient: ApiClient(
              config: const ApiConfig(baseUrl: 'http://localhost:5000'),
              tokenStore: tokenStore,
              httpClient: MockClient((request) async {
                if (request.url.path == '/games') {
                  return http.Response('{"id":"game-1"}', 201);
                }

                return http.Response('', 204);
              }),
            ),
          ),
        );
      },
      act: (cubit) => cubit.startGame(categoryId: 'category-1'),
      expect: () => [
        const GameStartState.submitting(),
        const GameStartState.success(gameId: 'game-1'),
      ],
    );

    blocTest<GameStartCubit, GameStartState>(
      'fails when session is missing',
      build: () {
        final tokenStore = InMemoryTokenStore();

        return GameStartCubit(
          tokenStore: tokenStore,
          gameRepository: GameRepository(
            apiClient: ApiClient(
              config: const ApiConfig(baseUrl: 'http://localhost:5000'),
              tokenStore: tokenStore,
              httpClient: MockClient((_) async => http.Response('', 500)),
            ),
          ),
        );
      },
      act: (cubit) => cubit.startGame(categoryId: 'category-1'),
      expect: () => [
        const GameStartState.submitting(),
        const GameStartState.failure(message: 'Guest session is missing.'),
      ],
    );
  });
}
