import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/admin_audit/application/admin_audit_cubit.dart';
import 'package:lexilink_app/features/admin_audit/data/admin_audit_repository.dart';
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

AdminAuditRepository _repoFromScript(_Script s) => AdminAuditRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient((req) async => s.respond(req)),
      ),
    );

String _action(String id, {String? targetId}) => '{'
    '"id":"$id",'
    '"occurredOn":"2026-05-22T10:00:00Z",'
    '"adminUserId":"00000000-0000-0000-0000-aaaaaaaaaaaa",'
    '"actionType":"CreateCategoryCommand",'
    '"targetType":"Games.Category",'
    '"targetId":${targetId == null ? "null" : '"$targetId"'},'
    r'"payloadJson":"{\"name\":\"Spor\"}"'
    '}';

String _page(int count) {
  final items = [
    for (var i = 0; i < count; i++)
      _action('00000000-0000-0000-0000-${i.toString().padLeft(12, '0')}'),
  ].join(',');
  return '[$items]';
}

void main() {
  group('AdminAuditCubit', () {
    blocTest<AdminAuditCubit, AdminAuditState>(
      'load emits loaded with the page',
      build: () {
        final s = _Script()..enqueue((_) => http.Response(_page(2), 200));
        return AdminAuditCubit(repository: _repoFromScript(s));
      },
      act: (cubit) => cubit.load(),
      verify: (cubit) {
        expect(cubit.state.status, AdminAuditStatus.loaded);
        expect(cubit.state.actions, hasLength(2));
        expect(cubit.state.offset, 0);
        expect(cubit.state.hasMore, isFalse,
            reason: '2 actions < pageSize (50), no more pages');
      },
    );

    blocTest<AdminAuditCubit, AdminAuditState>(
      'load failure emits failure with message',
      build: () {
        final s = _Script()
          ..enqueue((_) => http.Response('{"detail":"boom"}', 500));
        return AdminAuditCubit(repository: _repoFromScript(s));
      },
      act: (cubit) => cubit.load(),
      verify: (cubit) {
        expect(cubit.state.status, AdminAuditStatus.failure);
      },
    );

    blocTest<AdminAuditCubit, AdminAuditState>(
      'applyFilter resets offset and re-fetches',
      build: () {
        final s = _Script()
          ..enqueue((req) {
            expect(req.url.queryParameters.containsKey('targetType'), isFalse);
            return http.Response(_page(1), 200);
          })
          ..enqueue((req) {
            expect(req.url.queryParameters['targetType'], 'Games.Category');
            expect(req.url.queryParameters['offset'], '0');
            return http.Response(_page(1), 200);
          });
        return AdminAuditCubit(repository: _repoFromScript(s));
      },
      act: (cubit) async {
        await cubit.load();
        await cubit.applyFilter(
          const AdminAuditFilter(targetType: 'Games.Category'),
        );
      },
      verify: (cubit) {
        expect(cubit.state.filter.targetType, 'Games.Category');
        expect(cubit.state.offset, 0);
      },
    );

    blocTest<AdminAuditCubit, AdminAuditState>(
      'nextPage advances offset by pageSize and tracks hasMore',
      build: () {
        final s = _Script()
          ..enqueue((req) {
            expect(req.url.queryParameters['offset'], '0');
            return http.Response(_page(2), 200);
          })
          ..enqueue((req) {
            expect(req.url.queryParameters['offset'], '2');
            return http.Response(_page(1), 200);
          });
        return AdminAuditCubit(
          repository: _repoFromScript(s),
          pageSize: 2,
        );
      },
      act: (cubit) async {
        await cubit.load();
        // After load: 2 actions returned with pageSize 2 → hasMore true.
        expect(cubit.state.hasMore, isTrue);
        await cubit.nextPage();
      },
      verify: (cubit) {
        expect(cubit.state.offset, 2);
        expect(cubit.state.hasMore, isFalse,
            reason: '1 < pageSize 2, no more pages');
      },
    );

    blocTest<AdminAuditCubit, AdminAuditState>(
      'prevPage at offset 0 is a no-op',
      build: () {
        final s = _Script()..enqueue((_) => http.Response(_page(1), 200));
        return AdminAuditCubit(repository: _repoFromScript(s));
      },
      act: (cubit) async {
        await cubit.load();
        await cubit.prevPage();
      },
      verify: (cubit) {
        expect(cubit.state.offset, 0);
      },
    );
  });
}
