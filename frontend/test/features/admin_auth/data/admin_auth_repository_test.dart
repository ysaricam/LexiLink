import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/admin_auth/data/admin_auth_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/api/api_error.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

void main() {
  group('AdminAuthRepository', () {
    test('exchanges external token for admin session', () async {
      final repository = AdminAuthRepository(
        apiClient: ApiClient(
          config: const ApiConfig(baseUrl: 'http://localhost:5000'),
          tokenStore: InMemoryTokenStore(),
          httpClient: MockClient((request) async {
            expect(request.url.path, '/auth/admin/token');
            expect(request.body, contains('"email":"ops@lexilink.test"'));
            expect(
              request.body,
              contains('"externalToken":"dev:admin:ops@lexilink.test"'),
            );

            return http.Response(
              '{'
              '"accessToken":"jwt-123",'
              '"expiresAt":"2026-05-21T12:00:00Z",'
              '"adminUserId":"00000000-0000-0000-0000-000000000001",'
              '"email":"ops@lexilink.test",'
              '"role":"Admin"'
              '}',
              200,
            );
          }),
        ),
      );

      final session = await repository.exchangeToken(
        email: 'ops@lexilink.test',
        externalToken: 'dev:admin:ops@lexilink.test',
      );

      expect(session.accessToken, 'jwt-123');
      expect(session.adminUserId, '00000000-0000-0000-0000-000000000001');
      expect(session.email, 'ops@lexilink.test');
      expect(session.role, 'Admin');
    });

    test('propagates ApiException on 401', () async {
      final repository = AdminAuthRepository(
        apiClient: ApiClient(
          config: const ApiConfig(baseUrl: 'http://localhost:5000'),
          tokenStore: InMemoryTokenStore(),
          httpClient: MockClient((_) async => http.Response('', 401)),
        ),
      );

      expect(
        () => repository.exchangeToken(
          email: 'ops@lexilink.test',
          externalToken: 'wrong',
        ),
        throwsA(isA<ApiException>()),
      );
    });
  });
}
