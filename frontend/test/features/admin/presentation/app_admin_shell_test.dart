import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:lexilink_app/features/admin/presentation/admin_placeholder_page.dart';
import 'package:lexilink_app/features/admin/presentation/app_admin_shell.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

GoRouter _buildRouter({
  required String initialLocation,
  required TokenStore tokenStore,
}) {
  return GoRouter(
    initialLocation: initialLocation,
    routes: [
      GoRoute(
        path: '/admin/login',
        builder: (_, _) => const Scaffold(body: Text('LOGIN_SCREEN')),
      ),
      ShellRoute(
        builder: (context, state, child) => AppAdminShell(
          location: state.uri.toString(),
          tokenStoreFactory: () async => tokenStore,
          child: child,
        ),
        routes: [
          GoRoute(
            path: '/admin/quests',
            builder: (_, _) => const AdminQuestsPage(),
          ),
          GoRoute(
            path: '/admin/players',
            builder: (_, _) => const AdminPlayersPage(),
          ),
          GoRoute(
            path: '/admin/energy',
            builder: (_, _) => const AdminEnergyPage(),
          ),
          GoRoute(
            path: '/admin/audit',
            builder: (_, _) => const AdminAuditPage(),
          ),
        ],
      ),
    ],
  );
}

Future<void> _pumpShell(
  WidgetTester tester, {
  required String initialLocation,
  required TokenStore tokenStore,
  Size size = const Size(1200, 800),
}) async {
  tester.view.physicalSize = size;
  tester.view.devicePixelRatio = 1.0;
  addTearDown(tester.view.resetPhysicalSize);
  addTearDown(tester.view.resetDevicePixelRatio);

  final router = _buildRouter(
    initialLocation: initialLocation,
    tokenStore: tokenStore,
  );

  await tester.pumpWidget(MaterialApp.router(routerConfig: router));
  // Let the token-store factory resolve.
  await tester.pump();
}

void main() {
  group('AppAdminShell', () {
    testWidgets('renders NavigationRail on wide viewports', (tester) async {
      // Start at a still-placeholder destination so the wide-layout
      // assertions don't accidentally exercise AdminQuestsScreen's
      // SharedPreferences plumbing.
      await _pumpShell(
        tester,
        initialLocation: '/admin/players',
        tokenStore: InMemoryTokenStore(),
      );

      expect(find.byType(NavigationRail), findsOneWidget);
      expect(find.byType(NavigationDrawer), findsNothing);
      expect(find.text('Quests'), findsWidgets);
      expect(find.text('Players'), findsWidgets);
      expect(find.text('Energy'), findsWidgets);
      expect(find.text('Audit'), findsWidgets);
    });

    testWidgets('renders AppBar on narrow viewports', (tester) async {
      await _pumpShell(
        tester,
        initialLocation: '/admin/players',
        tokenStore: InMemoryTokenStore(),
        size: const Size(400, 800),
      );

      expect(find.byType(NavigationRail), findsNothing);
      expect(find.byType(AppBar), findsOneWidget);
      expect(find.text('Admin · Players'), findsOneWidget);
    });

    testWidgets('tapping a rail destination navigates to that route',
        (tester) async {
      await _pumpShell(
        tester,
        initialLocation: '/admin/players',
        tokenStore: InMemoryTokenStore(),
      );

      expect(find.text('Coming in F4'), findsOneWidget);

      await tester.tap(find.text('Audit').first);
      await tester.pumpAndSettle();

      expect(find.text('Coming in F6'), findsOneWidget);
    });

    testWidgets('sign-out clears token store and routes to /admin/login',
        (tester) async {
      final store = InMemoryTokenStore();
      await store.saveAccessToken('jwt-admin');

      await _pumpShell(
        tester,
        initialLocation: '/admin/players',
        tokenStore: store,
      );
      // Give the shell's _resolveStore future a chance to land.
      await tester.pump();

      await tester.tap(find.byTooltip('Sign out'));
      await tester.pumpAndSettle();

      expect(await store.readAccessToken(), isNull);
      expect(find.text('LOGIN_SCREEN'), findsOneWidget);
    });
  });
}
