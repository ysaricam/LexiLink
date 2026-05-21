import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/admin_auth/application/admin_session_cubit.dart';
import 'package:lexilink_app/features/admin_auth/data/admin_auth_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

AdminAuthRepository _repoReturning(http.Response Function() responder) {
  return AdminAuthRepository(
    apiClient: ApiClient(
      config: const ApiConfig(baseUrl: 'http://localhost:5000'),
      tokenStore: InMemoryTokenStore(),
      httpClient: MockClient((_) async => responder()),
    ),
  );
}

void main() {
  group('AdminSessionCubit', () {
    blocTest<AdminSessionCubit, AdminSessionState>(
      'checkSession emits unauthenticated when no stored token',
      build: () => AdminSessionCubit(
        repository: _repoReturning(() => http.Response('', 500)),
        adminTokenStore: InMemoryTokenStore(),
      ),
      act: (cubit) => cubit.checkSession(),
      expect: () => [const AdminSessionState.unauthenticated()],
    );

    test('checkSession restores authenticated state from stored token',
        () async {
      final store = InMemoryTokenStore();
      await store.saveAccessToken('jwt-restored');
      final cubit = AdminSessionCubit(
        repository: _repoReturning(() => http.Response('', 500)),
        adminTokenStore: store,
      );

      await cubit.checkSession();

      expect(cubit.state.status, AdminSessionStatus.authenticated);
      expect(cubit.state.accessToken, 'jwt-restored');
    });

    blocTest<AdminSessionCubit, AdminSessionState>(
      'signIn happy path stores token and emits authenticated',
      build: () {
        const body = '{'
            '"accessToken":"jwt-1",'
            '"expiresAt":"2026-05-21T12:00:00Z",'
            '"adminUserId":"00000000-0000-0000-0000-000000000001",'
            '"email":"ops@lexilink.test",'
            '"role":"Admin"'
            '}';
        return AdminSessionCubit(
          repository: _repoReturning(() => http.Response(body, 200)),
          adminTokenStore: InMemoryTokenStore(),
        );
      },
      act: (cubit) => cubit.signIn(
        email: 'ops@lexilink.test',
        externalToken: 'dev:admin:ops@lexilink.test',
      ),
      verify: (cubit) {
        expect(cubit.state.status, AdminSessionStatus.authenticated);
        expect(cubit.state.accessToken, 'jwt-1');
        expect(cubit.state.session?.email, 'ops@lexilink.test');
      },
    );

    blocTest<AdminSessionCubit, AdminSessionState>(
      'signIn on 401 emits failure with friendly message',
      build: () => AdminSessionCubit(
        repository: _repoReturning(() => http.Response('', 401)),
        adminTokenStore: InMemoryTokenStore(),
      ),
      act: (cubit) => cubit.signIn(email: 'x@y.test', externalToken: 'bad'),
      verify: (cubit) {
        expect(cubit.state.status, AdminSessionStatus.failure);
        expect(
          cubit.state.errorMessage,
          'External token rejected. Check email and dev token.',
        );
      },
    );

    blocTest<AdminSessionCubit, AdminSessionState>(
      'signIn on 404 reports no-such-admin message',
      build: () => AdminSessionCubit(
        repository: _repoReturning(() => http.Response('', 404)),
        adminTokenStore: InMemoryTokenStore(),
      ),
      act: (cubit) => cubit.signIn(email: 'ghost@y.test', externalToken: 'x'),
      verify: (cubit) {
        expect(cubit.state.errorMessage,
            'No active admin user is registered for this email.');
      },
    );

    blocTest<AdminSessionCubit, AdminSessionState>(
      'signOut clears token and emits unauthenticated',
      build: () => AdminSessionCubit(
        repository: _repoReturning(() => http.Response('', 500)),
        adminTokenStore: InMemoryTokenStore(),
      ),
      seed: () => const AdminSessionState.authenticated(accessToken: 'jwt-1'),
      act: (cubit) => cubit.signOut(),
      expect: () => [const AdminSessionState.unauthenticated()],
    );
  });
}
