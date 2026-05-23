import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/admin_energy/application/admin_energy_cubit.dart';
import 'package:lexilink_app/features/admin_energy/data/admin_energy_repository.dart';
import 'package:lexilink_app/features/admin_energy/presentation/admin_energy_screen.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

AdminEnergyCubit _buildCubitWithScript(
  List<http.Response Function(http.Request)> steps,
) {
  var i = 0;
  return AdminEnergyCubit(
    repository: AdminEnergyRepository(
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

const _snapshotJson = '{'
    '"playerId":"00000000-0000-0000-0000-000000000abc",'
    '"currentAmount":3,"maximumAmount":5,"isFull":false,'
    '"rechargeIntervalSeconds":600,'
    '"lastRefilledOn":"2026-05-22T09:00:00Z",'
    '"secondsUntilNextRefill":300,'
    '"fullyRefilledAt":"2026-05-22T11:00:00Z"'
    '}';

void main() {
  testWidgets('lookup submits and renders the energy card', (tester) async {
    tester.view.physicalSize = const Size(1200, 800);
    tester.view.devicePixelRatio = 1.0;
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);

    final cubit = _buildCubitWithScript([
      (_) => http.Response(_snapshotJson, 200),
    ]);

    await tester.pumpWidget(MaterialApp(
      home: Scaffold(
        body: AdminEnergyScreen(cubitFactory: () => cubit),
      ),
    ));
    await tester.pumpAndSettle();

    await tester.enterText(
      find.byType(TextField),
      '00000000-0000-0000-0000-000000000abc',
    );
    await tester.tap(find.text('Look up'));
    await tester.pumpAndSettle();

    expect(find.text('3 / 5'), findsOneWidget);
    expect(find.text('Set amount'), findsOneWidget);
    expect(find.text('Grant bonus'), findsOneWidget);
    expect(find.text('Reset to full'), findsOneWidget);
  });

  testWidgets('notFound state shows a friendly message', (tester) async {
    tester.view.physicalSize = const Size(1200, 800);
    tester.view.devicePixelRatio = 1.0;
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);

    final cubit = _buildCubitWithScript([
      (_) => http.Response('{"detail":"nope"}', 404),
    ]);

    await tester.pumpWidget(MaterialApp(
      home: Scaffold(
        body: AdminEnergyScreen(cubitFactory: () => cubit),
      ),
    ));
    await tester.pumpAndSettle();

    await tester.enterText(find.byType(TextField), 'ghost');
    await tester.tap(find.text('Look up'));
    await tester.pumpAndSettle();

    expect(find.textContaining('No energy aggregate'), findsOneWidget);
  });
}
