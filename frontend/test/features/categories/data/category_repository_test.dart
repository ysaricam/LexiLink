import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/categories/data/category_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

void main() {
  test('gets categories', () async {
    final repository = CategoryRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient((request) async {
          expect(request.url.path, '/categories');

          return http.Response(
            '[{"id":"category-1","name":"Animals"}]',
            200,
          );
        }),
      ),
    );

    final categories = await repository.getCategories();

    expect(categories, hasLength(1));
    expect(categories.single.id, 'category-1');
    expect(categories.single.name, 'Animals');
  });
}
