import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/diamond/data/diamond_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

void main() {
  test('getMe calls /diamond/me and decodes balance', () async {
    final repository = DiamondRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient((request) async {
          expect(request.method, 'GET');
          expect(request.url.path, '/diamond/me');
          return http.Response(
            '{"playerId":"00000000-0000-0000-0000-000000000abc","balance":9}',
            200,
          );
        }),
      ),
    );

    final diamond = await repository.getMe();

    expect(diamond.balance, 9);
  });
}
