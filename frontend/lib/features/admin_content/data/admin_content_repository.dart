import 'package:lexilink_app/features/admin_content/data/admin_content_models.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class AdminContentRepository {
  const AdminContentRepository({required ApiClient apiClient})
    : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<List<AdminContentCategory>> fetchCategories({
    String? locale,
  }) async {
    final raw = await _apiClient.getJsonList(
      '/admin/content/categories',
      queryParameters: locale == null ? null : {'locale': locale},
    );

    return raw
        .cast<Map<String, dynamic>>()
        .map(AdminContentCategory.fromJson)
        .toList(growable: false);
  }

  Future<AdminContentCategoryDetail> fetchCategory(String id) async {
    final raw = await _apiClient.getJson('/admin/content/categories/$id');
    return AdminContentCategoryDetail.fromJson(raw);
  }

  Future<void> createCategory({
    required String name,
    required String description,
    required String language,
  }) async {
    await _apiClient.postJson(
      '/admin/content/categories',
      body: {
        'name': name,
        'description': description,
        'language': language,
      },
    );
  }

  Future<void> updateCategory({
    required String id,
    required String name,
    required String description,
    required String language,
  }) async {
    await _apiClient.patchJson(
      '/admin/content/categories/$id',
      body: {
        'name': name,
        'description': description,
        'language': language,
      },
    );
  }
}
