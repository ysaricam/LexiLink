import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/admin_quests/application/admin_quests_cubit.dart';
import 'package:lexilink_app/features/admin_quests/data/admin_quests_repository.dart';
import 'package:lexilink_app/features/admin_quests/presentation/admin_quests_screen.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

AdminQuestsCubit _buildCubitWithScript(
  List<http.Response Function(http.Request)> steps,
) {
  var i = 0;
  return AdminQuestsCubit(
    repository: AdminQuestsRepository(
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

void main() {
  testWidgets('renders quest list and opens create dialog from FAB',
      (tester) async {
    tester.view.physicalSize = const Size(1200, 800);
    tester.view.devicePixelRatio = 1.0;
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);

    final cubit = _buildCubitWithScript([
      (_) => http.Response(
            '['
            '{"id":"00000000-0000-0000-0000-000000000001",'
            '"questType":"DailyThreeGames","cadence":"Daily",'
            '"goal":3,"rewardAmount":15,'
            '"prerequisiteQuestType":null,"isActive":true}'
            ']',
            200,
          ),
    ]);

    await tester.pumpWidget(MaterialApp(
      home: Scaffold(
        body: AdminQuestsScreen(cubitFactory: () => cubit),
      ),
    ));
    // Async init + load completes.
    await tester.pumpAndSettle();

    expect(find.text('Quest definitions'), findsOneWidget);
    expect(find.text('DailyThreeGames'), findsOneWidget);
    expect(find.text('Daily'), findsOneWidget);

    await tester.tap(find.text('New quest'));
    await tester.pumpAndSettle();

    expect(find.text('New quest definition'), findsOneWidget);
    expect(find.text('Cancel'), findsOneWidget);
    expect(find.text('Create'), findsOneWidget);
  });

  testWidgets('empty state shows hint message', (tester) async {
    tester.view.physicalSize = const Size(1200, 800);
    tester.view.devicePixelRatio = 1.0;
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);

    final cubit = _buildCubitWithScript([
      (_) => http.Response('[]', 200),
    ]);

    await tester.pumpWidget(MaterialApp(
      home: Scaffold(
        body: AdminQuestsScreen(cubitFactory: () => cubit),
      ),
    ));
    await tester.pumpAndSettle();

    expect(
      find.textContaining('No quest definitions yet'),
      findsOneWidget,
    );
  });
}
