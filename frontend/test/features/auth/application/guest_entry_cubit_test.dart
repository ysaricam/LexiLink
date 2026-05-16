import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/auth/application/guest_entry_cubit.dart';
import 'package:lexilink_app/features/auth/data/guest_player_repository.dart';
import 'package:lexilink_app/features/session/application/session_cubit.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

void main() {
  group('GuestEntryCubit', () {
    blocTest<GuestEntryCubit, GuestEntryState>(
      'registers guest and authenticates session',
      build: () {
        final tokenStore = InMemoryTokenStore();
        final sessionCubit = SessionCubit(tokenStore: tokenStore);
        final apiClient = ApiClient(
          config: const ApiConfig(baseUrl: 'http://localhost:5000'),
          tokenStore: tokenStore,
          httpClient: MockClient(
            (request) async => http.Response('{"id":"player-1"}', 201),
          ),
        );

        return GuestEntryCubit(
          guestPlayerRepository: GuestPlayerRepository(apiClient: apiClient),
          sessionCubit: sessionCubit,
        );
      },
      act: (cubit) => cubit.continueAsGuest(
        deviceId: 'device-1',
        displayName: 'Guest Player',
        locale: 'en-US',
      ),
      expect: () => [
        const GuestEntryState.submitting(),
        const GuestEntryState.success(playerId: 'player-1'),
      ],
    );

    blocTest<GuestEntryCubit, GuestEntryState>(
      'resets guest entry state',
      build: () {
        final tokenStore = InMemoryTokenStore();
        final sessionCubit = SessionCubit(tokenStore: tokenStore);
        final apiClient = ApiClient(
          config: const ApiConfig(baseUrl: 'http://localhost:5000'),
          tokenStore: tokenStore,
          httpClient: MockClient(
            (request) async => http.Response('{"id":"player-1"}', 201),
          ),
        );

        return GuestEntryCubit(
          guestPlayerRepository: GuestPlayerRepository(apiClient: apiClient),
          sessionCubit: sessionCubit,
        );
      },
      seed: () => const GuestEntryState.success(playerId: 'player-1'),
      act: (cubit) => cubit.reset(),
      expect: () => [const GuestEntryState.idle()],
    );

    test('writes player id to session token store', () async {
      final tokenStore = InMemoryTokenStore();
      final sessionCubit = SessionCubit(tokenStore: tokenStore);
      final apiClient = ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: tokenStore,
        httpClient: MockClient(
          (request) async => http.Response('{"id":"player-1"}', 201),
        ),
      );
      final cubit = GuestEntryCubit(
        guestPlayerRepository: GuestPlayerRepository(apiClient: apiClient),
        sessionCubit: sessionCubit,
      );

      await cubit.continueAsGuest(
        deviceId: 'device-1',
        displayName: 'Guest Player',
        locale: 'en-US',
      );

      expect(tokenStore.accessToken, 'player-1');
      expect(sessionCubit.state.status, SessionStatus.authenticated);
    });
  });
}
