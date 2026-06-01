import 'package:lexilink_app/features/categories/data/category.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class CategoryRepository {
  const CategoryRepository({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<List<Category>> getCategories({required String locale}) async {
    final response = await _apiClient.getJsonList(
      '/categories',
      queryParameters: {'locale': locale},
    );

    return response
        .map((item) {
          if (item is Map<String, dynamic>) {
            return Category.fromJson(item);
          }

          throw StateError('Category response contains an invalid item.');
        })
        .toList(growable: false);
  }
}
