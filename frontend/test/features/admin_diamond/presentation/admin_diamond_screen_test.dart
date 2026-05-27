import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/admin_diamond/application/admin_diamond_cubit.dart';
import 'package:lexilink_app/features/admin_diamond/data/admin_diamond_repository.dart';
import 'package:lexilink_app/features/admin_diamond/presentation/admin_diamond_screen.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

AdminDiamondCubit _buildCubitWithScript(
  List<http.Response Function(http.Request)> steps,
) {
  var i = 0;
  return AdminDiamondCubit(
    repository: AdminDiamondRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: MockClient((req) async {
          if (i >= steps.length) {
            fail('Unexpected HTTP call: ${req.method} ${req.url.path}');
          }
          return steps[i++](req);
        }),
      ),
    ),
  );
}

const _playerId = '00000000-0000-0000-0000-000000000abc';

String _snapshotJson(int balance) =>
    '{"playerId":"$_playerId","balance":$balance}';

void main() {
  testWidgets('set balance dialog keeps its controller alive until submit', (
    tester,
  ) async {
    tester.view.physicalSize = const Size(1200, 800);
    tester.view.devicePixelRatio = 1.0;
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);

    final cubit = _buildCubitWithScript([
      (req) {
        expect(req.method, 'GET');
        return http.Response(_snapshotJson(7), 200);
      },
      (req) {
        expect(req.method, 'POST');
        expect(req.url.path, '/admin/players/$_playerId/diamond/set');
        expect(req.body, contains('"balance":11'));
        return http.Response('', 204);
      },
      (req) {
        expect(req.method, 'GET');
        return http.Response(_snapshotJson(11), 200);
      },
    ]);

    await tester.pumpWidget(
      MaterialApp(
        home: Scaffold(
          body: AdminDiamondScreen(cubitFactory: () => cubit),
        ),
      ),
    );
    await tester.pumpAndSettle();

    await tester.enterText(find.byType(TextField), _playerId);
    await tester.tap(find.text('Look up'));
    await tester.pumpAndSettle();

    await tester.tap(find.text('Set balance'));
    await tester.pumpAndSettle();
    await tester.enterText(find.byType(TextField).last, '11');
    await tester.tap(find.text('Apply'));
    await tester.pumpAndSettle();

    expect(tester.takeException(), isNull);
    expect(find.text('11'), findsOneWidget);
  });
}
