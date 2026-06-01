import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/admin_audit/application/admin_audit_cubit.dart';
import 'package:lexilink_app/features/admin_audit/data/admin_audit_repository.dart';
import 'package:lexilink_app/features/admin_audit/presentation/admin_audit_screen.dart';
import 'package:lexilink_app/l10n/app_localizations.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

AdminAuditCubit _buildCubitWithScript(
  List<http.Response Function(http.Request)> steps,
) {
  var i = 0;
  return AdminAuditCubit(
    repository: AdminAuditRepository(
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

const _oneActionJson =
    '['
    '{"id":"00000000-0000-0000-0000-000000000001",'
    '"occurredOn":"2026-05-22T10:00:00Z",'
    '"adminUserId":"00000000-0000-0000-0000-aaaaaaaaaaaa",'
    '"actionType":"BanPlayerCommand",'
    '"targetType":"Players.Player",'
    '"targetId":"00000000-0000-0000-0000-bbbbbbbbbbbb",'
    r'"payloadJson":"{\"reason\":\"spam\"}"}'
    ']';

void main() {
  testWidgets('renders the audit row and opens payload dialog', (tester) async {
    tester.view.physicalSize = const Size(1200, 800);
    tester.view.devicePixelRatio = 1.0;
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);

    final cubit = _buildCubitWithScript([
      (_) => http.Response(_oneActionJson, 200),
    ]);

    await tester.pumpWidget(
      MaterialApp(
        localizationsDelegates: AppLocalizations.localizationsDelegates,
        supportedLocales: AppLocalizations.supportedLocales,
        home: Scaffold(
          body: AdminAuditScreen(cubitFactory: () => cubit),
        ),
      ),
    );
    await tester.pumpAndSettle();

    expect(find.text('Audit log'), findsOneWidget);
    expect(find.text('BanPlayerCommand'), findsOneWidget);
    expect(find.textContaining('Players.Player'), findsOneWidget);

    await tester.tap(find.byTooltip('View payload'));
    await tester.pumpAndSettle();

    expect(find.textContaining('reason'), findsWidgets);
    expect(find.textContaining('spam'), findsWidgets);
  });

  testWidgets('empty page shows the friendly message', (tester) async {
    tester.view.physicalSize = const Size(1200, 800);
    tester.view.devicePixelRatio = 1.0;
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);

    final cubit = _buildCubitWithScript([
      (_) => http.Response('[]', 200),
    ]);

    await tester.pumpWidget(
      MaterialApp(
        localizationsDelegates: AppLocalizations.localizationsDelegates,
        supportedLocales: AppLocalizations.supportedLocales,
        home: Scaffold(
          body: AdminAuditScreen(cubitFactory: () => cubit),
        ),
      ),
    );
    await tester.pumpAndSettle();

    expect(
      find.textContaining('No audit entries'),
      findsOneWidget,
    );
  });
}
