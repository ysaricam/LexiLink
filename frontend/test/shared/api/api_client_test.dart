import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/api/api_error.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

void main() {
  group('ApiClient', () {
    test('adds bearer token when available', () async {
      final tokenStore = InMemoryTokenStore();
      await tokenStore.saveAccessToken('abc123');

      final client = ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: tokenStore,
        httpClient: MockClient((request) async {
          expect(request.headers['authorization'], 'Bearer abc123');
          expect(request.headers['accept'], 'application/json');

          return http.Response('{"ok":true}', 200);
        }),
      );

      final result = await client.getJson('/health');

      expect(result['ok'], isTrue);
    });

    test('maps ProblemDetails responses', () async {
      final client = ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient(
          (_) async => http.Response(
            '''
{"title":"Resource not found","detail":"Player was not found.","status":404,"traceId":"trace-1"}
''',
            404,
            headers: {'content-type': 'application/problem+json'},
          ),
        ),
      );

      await expectLater(
        client.getJson('/missing'),
        throwsA(
          isA<ApiException>()
              .having((error) => error.statusCode, 'statusCode', 404)
              .having(
                (error) => error.message,
                'message',
                'Player was not found.',
              )
              .having(
                (error) => error.problem?.traceId,
                'traceId',
                'trace-1',
              ),
        ),
      );
    });

    test('decodes JSON list responses', () async {
      final client = ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient(
          (_) async => http.Response('[{"id":"category-1"}]', 200),
        ),
      );

      final result = await client.getJsonList('/categories');

      expect(result, hasLength(1));
      expect((result.single as Map<String, dynamic>)['id'], 'category-1');
    });

    test('maps unauthorized responses', () async {
      final client = ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient((_) async => http.Response('', 401)),
      );

      await expectLater(
        client.getJson('/protected'),
        throwsA(
          isA<ApiException>()
              .having((error) => error.isUnauthorized, 'isUnauthorized', isTrue)
              .having(
                (error) => error.message,
                'message',
                'Authentication is required.',
              ),
        ),
      );
    });
  });
}
