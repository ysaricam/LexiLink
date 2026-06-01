import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/categories/application/category_list_cubit.dart';
import 'package:lexilink_app/features/categories/data/category.dart';
import 'package:lexilink_app/features/categories/data/category_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

void main() {
  group('CategoryListCubit', () {
    blocTest<CategoryListCubit, CategoryListState>(
      'loads categories',
      build: () {
        final repository = CategoryRepository(
          apiClient: ApiClient(
            config: const ApiConfig(baseUrl: 'http://localhost:5000'),
            tokenStore: InMemoryTokenStore(),
            httpClient: MockClient(
              (_) async => http.Response(
                '[{"id":"category-1","name":"Animals","language":"en-US"}]',
                200,
              ),
            ),
          ),
        );

        return CategoryListCubit(categoryRepository: repository);
      },
      act: (cubit) => cubit.loadCategories(locale: 'en-US'),
      expect: () => [
        const CategoryListState.loading(),
        const CategoryListState.success(
          categories: [
            Category(id: 'category-1', name: 'Animals', language: 'en-US'),
          ],
        ),
      ],
    );

    blocTest<CategoryListCubit, CategoryListState>(
      'emits failure when request fails',
      build: () {
        final repository = CategoryRepository(
          apiClient: ApiClient(
            config: const ApiConfig(baseUrl: 'http://localhost:5000'),
            tokenStore: InMemoryTokenStore(),
            httpClient: MockClient((_) async => http.Response('', 401)),
          ),
        );

        return CategoryListCubit(categoryRepository: repository);
      },
      act: (cubit) => cubit.loadCategories(locale: 'en-US'),
      expect: () => [
        const CategoryListState.loading(),
        const CategoryListState.failure(message: 'Authentication is required.'),
      ],
    );

    blocTest<CategoryListCubit, CategoryListState>(
      'selects loaded category',
      build: () {
        final repository = CategoryRepository(
          apiClient: ApiClient(
            config: const ApiConfig(baseUrl: 'http://localhost:5000'),
            tokenStore: InMemoryTokenStore(),
            httpClient: MockClient(
              (_) async => http.Response(
                '[{"id":"category-1","name":"Animals","language":"en-US"}]',
                200,
              ),
            ),
          ),
        );

        return CategoryListCubit(categoryRepository: repository);
      },
      act: (cubit) async {
        await cubit.loadCategories(locale: 'en-US');
        cubit.selectCategory('category-1');
      },
      expect: () => [
        const CategoryListState.loading(),
        const CategoryListState.success(
          categories: [
            Category(id: 'category-1', name: 'Animals', language: 'en-US'),
          ],
        ),
        const CategoryListState(
          status: CategoryListStatus.success,
          categories: [
            Category(id: 'category-1', name: 'Animals', language: 'en-US'),
          ],
          selectedCategoryId: 'category-1',
        ),
      ],
    );
  });
}
