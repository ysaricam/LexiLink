import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/admin_players/application/admin_players_cubit.dart';
import 'package:lexilink_app/features/admin_players/data/admin_players_repository.dart';
import 'package:lexilink_app/features/admin_players/presentation/admin_players_screen.dart';
import 'package:lexilink_app/l10n/app_localizations.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

AdminPlayersCubit _buildCubitWithScript(
  List<http.Response Function(http.Request)> steps,
) {
  var i = 0;
  return AdminPlayersCubit(
    repository: AdminPlayersRepository(
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

const _activePlayerJson =
    '{'
    '"id":"00000000-0000-0000-0000-000000000abc",'
    '"displayName":"Ada","discriminator":42,'
    '"handle":"ada","avatarUrl":null,'
    '"locale":"tr-TR","isGuest":false,'
    '"isBanned":false,'
    '"bannedReason":null,"bannedAt":null,'
    '"createdAt":"2026-01-01T00:00:00Z",'
    '"authProvidersLinked":1'
    '}';

void main() {
  testWidgets('initial state shows the lookup hint, not a detail card', (
    tester,
  ) async {
    tester.view.physicalSize = const Size(1200, 800);
    tester.view.devicePixelRatio = 1.0;
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);

    final cubit = _buildCubitWithScript([]);

    await tester.pumpWidget(
      MaterialApp(
        localizationsDelegates: AppLocalizations.localizationsDelegates,
        supportedLocales: AppLocalizations.supportedLocales,
        home: Scaffold(
          body: AdminPlayersScreen(cubitFactory: () => cubit),
        ),
      ),
    );
    await tester.pumpAndSettle();

    expect(find.text('Player console'), findsOneWidget);
    expect(find.text('Look up'), findsOneWidget);
    expect(find.byType(Card), findsNothing);
  });

  testWidgets('lookup submits and renders the active player card', (
    tester,
  ) async {
    tester.view.physicalSize = const Size(1200, 800);
    tester.view.devicePixelRatio = 1.0;
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);

    final cubit = _buildCubitWithScript([
      (_) => http.Response(_activePlayerJson, 200),
    ]);

    await tester.pumpWidget(
      MaterialApp(
        localizationsDelegates: AppLocalizations.localizationsDelegates,
        supportedLocales: AppLocalizations.supportedLocales,
        home: Scaffold(
          body: AdminPlayersScreen(cubitFactory: () => cubit),
        ),
      ),
    );
    await tester.pumpAndSettle();

    await tester.enterText(
      find.byType(TextField),
      '00000000-0000-0000-0000-000000000abc',
    );
    await tester.tap(find.text('Look up'));
    await tester.pumpAndSettle();

    expect(find.text('Ada'), findsOneWidget);
    expect(find.text('ada#42'), findsOneWidget);
    expect(find.widgetWithText(FilledButton, 'Ban'), findsOneWidget);
  });

  testWidgets('notFound state shows a friendly message', (tester) async {
    tester.view.physicalSize = const Size(1200, 800);
    tester.view.devicePixelRatio = 1.0;
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);

    final cubit = _buildCubitWithScript([
      (_) => http.Response('{"detail":"nope"}', 404),
    ]);

    await tester.pumpWidget(
      MaterialApp(
        localizationsDelegates: AppLocalizations.localizationsDelegates,
        supportedLocales: AppLocalizations.supportedLocales,
        home: Scaffold(
          body: AdminPlayersScreen(cubitFactory: () => cubit),
        ),
      ),
    );
    await tester.pumpAndSettle();

    await tester.enterText(
      find.byType(TextField),
      '00000000-0000-0000-0000-000000000ghost',
    );
    await tester.tap(find.text('Look up'));
    await tester.pumpAndSettle();

    expect(find.textContaining('No player with id'), findsOneWidget);
  });
}
