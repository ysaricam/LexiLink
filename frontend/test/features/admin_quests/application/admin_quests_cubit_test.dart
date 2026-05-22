import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/admin_quests/application/admin_quests_cubit.dart';
import 'package:lexilink_app/features/admin_quests/data/admin_quests_repository.dart';
import 'package:lexilink_app/features/admin_quests/data/quest_enums.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

/// Drives the cubit's underlying repository via a scripted MockClient.
class _Script {
  _Script();

  final List<http.Response Function(http.Request)> _steps = [];
  int _i = 0;

  void enqueue(http.Response Function(http.Request) responder) {
    _steps.add(responder);
  }

  http.Response respond(http.Request req) {
    if (_i >= _steps.length) {
      fail('Unexpected HTTP call #${_i + 1}: ${req.method} ${req.url.path}');
    }
    final responder = _steps[_i++];
    return responder(req);
  }
}

AdminQuestsRepository _repoFromScript(_Script script) {
  return AdminQuestsRepository(
    apiClient: ApiClient(
      config: const ApiConfig(baseUrl: 'http://localhost:5000'),
      tokenStore: InMemoryTokenStore(),
      httpClient: MockClient(
        (req) async => script.respond(req),
      ),
    ),
  );
}

const _emptyList = '[]';
const _oneDailyDef = '['
    '{"id":"00000000-0000-0000-0000-000000000001",'
    '"questType":"DailyThreeGames","cadence":"Daily",'
    '"goal":3,"rewardAmount":15,"prerequisiteQuestType":null,'
    '"isActive":true}'
    ']';

void main() {
  group('AdminQuestsCubit', () {
    blocTest<AdminQuestsCubit, AdminQuestsState>(
      'load happy path emits loading then loaded with definitions',
      build: () {
        final script = _Script()
          ..enqueue((_) => http.Response(_oneDailyDef, 200));
        return AdminQuestsCubit(repository: _repoFromScript(script));
      },
      act: (cubit) => cubit.load(),
      verify: (cubit) {
        expect(cubit.state.status, AdminQuestsStatus.loaded);
        expect(cubit.state.definitions, hasLength(1));
        expect(
          cubit.state.definitions.first.questType,
          AdminQuestType.dailyThreeGames,
        );
      },
    );

    blocTest<AdminQuestsCubit, AdminQuestsState>(
      'load failure emits failure with message',
      build: () {
        final script = _Script()
          ..enqueue(
            (_) => http.Response('{"detail":"boom"}', 500),
          );
        return AdminQuestsCubit(repository: _repoFromScript(script));
      },
      act: (cubit) => cubit.load(),
      verify: (cubit) {
        expect(cubit.state.status, AdminQuestsStatus.failure);
        expect(cubit.state.errorMessage, isNotNull);
      },
    );

    blocTest<AdminQuestsCubit, AdminQuestsState>(
      'create reloads list after success',
      build: () {
        final script = _Script()
          ..enqueue((_) => http.Response(
                '{"id":"00000000-0000-0000-0000-000000000099"}',
                201,
              ))
          ..enqueue((_) => http.Response(_oneDailyDef, 200));
        return AdminQuestsCubit(repository: _repoFromScript(script));
      },
      act: (cubit) => cubit.create(
        questType: AdminQuestType.dailyThreeGames,
        cadence: AdminQuestCadence.daily,
        goal: 3,
        rewardAmount: 15,
      ),
      verify: (cubit) {
        expect(cubit.state.status, AdminQuestsStatus.loaded);
        expect(cubit.state.definitions, hasLength(1));
      },
    );

    blocTest<AdminQuestsCubit, AdminQuestsState>(
      'deactivate reloads list and surfaces inactive row',
      build: () {
        final script = _Script()
          ..enqueue((_) => http.Response('', 204))
          ..enqueue((_) => http.Response(
                '['
                '{"id":"00000000-0000-0000-0000-000000000001",'
                '"questType":"DailyThreeGames","cadence":"Daily",'
                '"goal":3,"rewardAmount":15,'
                '"prerequisiteQuestType":null,'
                '"isActive":false}'
                ']',
                200,
              ));
        return AdminQuestsCubit(repository: _repoFromScript(script));
      },
      act: (cubit) => cubit
          .deactivate('00000000-0000-0000-0000-000000000001'),
      verify: (cubit) {
        expect(cubit.state.status, AdminQuestsStatus.loaded);
        expect(cubit.state.definitions.first.isActive, isFalse);
      },
    );

    blocTest<AdminQuestsCubit, AdminQuestsState>(
      'update reloads list',
      build: () {
        final script = _Script()
          ..enqueue((_) => http.Response('', 204))
          ..enqueue((_) => http.Response(_emptyList, 200));
        return AdminQuestsCubit(repository: _repoFromScript(script));
      },
      act: (cubit) => cubit.update(
        id: '00000000-0000-0000-0000-000000000001',
        goal: 5,
        rewardAmount: 25,
      ),
      verify: (cubit) {
        expect(cubit.state.status, AdminQuestsStatus.loaded);
      },
    );
  });
}
