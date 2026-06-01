import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/admin_content/data/admin_content_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

AdminContentRepository _repo(MockClient client) => AdminContentRepository(
  apiClient: ApiClient(
    config: const ApiConfig(baseUrl: 'http://localhost:5000'),
    tokenStore: InMemoryTokenStore(),
    httpClient: client,
  ),
);

void main() {
  group('AdminContentRepository', () {
    test('fetchCategories passes locale and decodes categories', () async {
      final repo = _repo(
        MockClient((req) async {
          expect(req.method, 'GET');
          expect(req.url.path, '/admin/content/categories');
          expect(req.url.queryParameters['locale'], 'en-US');
          return http.Response(
            '[{"id":"c1","name":"Animals","language":"en-US"}]',
            200,
          );
        }),
      );

      final categories = await repo.fetchCategories(locale: 'en-US');

      expect(categories, hasLength(1));
      expect(categories.single.name, 'Animals');
      expect(categories.single.language, 'en-US');
    });

    test('fetchCategory decodes category details', () async {
      final repo = _repo(
        MockClient((req) async {
          expect(req.method, 'GET');
          expect(req.url.path, '/admin/content/categories/c1');
          return http.Response(
            jsonEncode({
              'id': 'c1',
              'name': 'Animals',
              'description': 'Animal words',
              'language': 'en-US',
              'linkCount': 12,
            }),
            200,
          );
        }),
      );

      final category = await repo.fetchCategory('c1');

      expect(category.name, 'Animals');
      expect(category.description, 'Animal words');
      expect(category.language, 'en-US');
      expect(category.linkCount, 12);
    });

    test('createCategory sends language in the request body', () async {
      final repo = _repo(
        MockClient((req) async {
          expect(req.method, 'POST');
          expect(req.url.path, '/admin/content/categories');
          final body = jsonDecode(req.body) as Map<String, dynamic>;
          expect(body['name'], 'Animaux');
          expect(body['description'], 'Mots animaux');
          expect(body['language'], 'fr-FR');
          return http.Response('{"id":"c1"}', 201);
        }),
      );

      await repo.createCategory(
        name: 'Animaux',
        description: 'Mots animaux',
        language: 'fr-FR',
      );
    });

    test('updateCategory sends language in the request body', () async {
      final repo = _repo(
        MockClient((req) async {
          expect(req.method, 'PATCH');
          expect(req.url.path, '/admin/content/categories/c1');
          final body = jsonDecode(req.body) as Map<String, dynamic>;
          expect(body['language'], 'de-DE');
          return http.Response('', 204);
        }),
      );

      await repo.updateCategory(
        id: 'c1',
        name: 'Tiere',
        description: 'Tierwoerter',
        language: 'de-DE',
      );
    });
  });
}
