import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/admin_content/application/admin_content_cubit.dart';
import 'package:lexilink_app/features/admin_content/data/admin_content_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

class _Script {
  final List<http.Response Function(http.Request)> _steps = [];
  int _i = 0;

  void enqueue(http.Response Function(http.Request) r) => _steps.add(r);

  http.Response respond(http.Request req) {
    if (_i >= _steps.length) {
      fail('Unexpected HTTP call: ${req.method} ${req.url.path}');
    }
    return _steps[_i++](req);
  }
}

AdminContentRepository _repoFromScript(_Script script) {
  return AdminContentRepository(
    apiClient: ApiClient(
      config: const ApiConfig(baseUrl: 'http://localhost:5000'),
      tokenStore: InMemoryTokenStore(),
      httpClient: MockClient((req) async => script.respond(req)),
    ),
  );
}

void main() {
  group('AdminContentCubit', () {
    blocTest<AdminContentCubit, AdminContentState>(
      'load emits localized category list',
      build: () {
        final script = _Script()
          ..enqueue((req) {
            expect(req.url.queryParameters['locale'], 'en-US');
            return http.Response(
              '[{"id":"c1","name":"Animals","language":"en-US"}]',
              200,
            );
          });
        return AdminContentCubit(repository: _repoFromScript(script));
      },
      act: (cubit) => cubit.load(locale: 'en-US'),
      verify: (cubit) {
        expect(cubit.state.status, AdminContentStatus.loaded);
        expect(cubit.state.localeFilter, 'en-US');
        expect(cubit.state.categories.single.name, 'Animals');
      },
    );

    blocTest<AdminContentCubit, AdminContentState>(
      'saveCategory reloads with the active locale filter',
      build: () {
        final script = _Script()
          ..enqueue(
            (_) => http.Response(
              '[{"id":"c1","name":"Animals","language":"en-US"}]',
              200,
            ),
          )
          ..enqueue((req) {
            expect(req.method, 'POST');
            return http.Response('{"id":"c2"}', 201);
          })
          ..enqueue((req) {
            expect(req.url.queryParameters['locale'], 'en-US');
            return http.Response(
              '[{"id":"c1","name":"Animals","language":"en-US"},'
              '{"id":"c2","name":"Colors","language":"en-US"}]',
              200,
            );
          });
        return AdminContentCubit(repository: _repoFromScript(script));
      },
      act: (cubit) async {
        await cubit.load(locale: 'en-US');
        await cubit.saveCategory(
          name: 'Colors',
          description: 'Color words',
          language: 'en-US',
        );
      },
      verify: (cubit) {
        expect(cubit.state.status, AdminContentStatus.loaded);
        expect(cubit.state.categories, hasLength(2));
      },
    );

    blocTest<AdminContentCubit, AdminContentState>(
      'failure exposes the API message',
      build: () {
        final script = _Script()
          ..enqueue((_) => http.Response('{"detail":"boom"}', 500));
        return AdminContentCubit(repository: _repoFromScript(script));
      },
      act: (cubit) => cubit.load(),
      verify: (cubit) {
        expect(cubit.state.status, AdminContentStatus.failure);
        expect(cubit.state.errorMessage, isNotEmpty);
      },
    );
  });
}
