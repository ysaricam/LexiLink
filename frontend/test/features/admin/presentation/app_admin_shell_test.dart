import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:lexilink_app/features/admin/presentation/app_admin_shell.dart';
import 'package:lexilink_app/l10n/app_localizations.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

/// Stub destination pages keep the shell widget test focused on
/// shell behavior (nav, sign-out) — the real admin destination pages
/// mount cubits that resolve SharedPreferences, which is not wired up
/// in widget tests.
Widget _stubPage(String marker) => Scaffold(body: Center(child: Text(marker)));

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
            builder: (_, _) => _stubPage('PAGE_QUESTS'),
          ),
          GoRoute(
            path: '/admin/players',
            builder: (_, _) => _stubPage('PAGE_PLAYERS'),
          ),
          GoRoute(
            path: '/admin/energy',
            builder: (_, _) => _stubPage('PAGE_ENERGY'),
          ),
          GoRoute(
            path: '/admin/hint',
            builder: (_, _) => _stubPage('PAGE_HINT'),
          ),
          GoRoute(
            path: '/admin/undo',
            builder: (_, _) => _stubPage('PAGE_UNDO'),
          ),
          GoRoute(
            path: '/admin/reset',
            builder: (_, _) => _stubPage('PAGE_RESET'),
          ),
          GoRoute(
            path: '/admin/diamond',
            builder: (_, _) => _stubPage('PAGE_DIAMOND'),
          ),
          GoRoute(
            path: '/admin/market',
            builder: (_, _) => _stubPage('PAGE_MARKET'),
          ),
          GoRoute(
            path: '/admin/content',
            builder: (_, _) => _stubPage('PAGE_CONTENT'),
          ),
          GoRoute(
            path: '/admin/audit',
            builder: (_, _) => _stubPage('PAGE_AUDIT'),
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

  await tester.pumpWidget(
    MaterialApp.router(
      localizationsDelegates: AppLocalizations.localizationsDelegates,
      supportedLocales: AppLocalizations.supportedLocales,
      routerConfig: router,
    ),
  );
  // Let the token-store factory resolve.
  await tester.pump();
}

void main() {
  group('AppAdminShell', () {
    testWidgets('renders NavigationRail on wide viewports', (tester) async {
      await _pumpShell(
        tester,
        initialLocation: '/admin/quests',
        tokenStore: InMemoryTokenStore(),
      );

      expect(find.byType(NavigationRail), findsOneWidget);
      expect(find.byType(NavigationDrawer), findsNothing);
      expect(find.text('Quests'), findsWidgets);
      expect(find.text('Players'), findsWidgets);
      expect(find.text('Energy'), findsWidgets);
      expect(find.text('Diamond'), findsWidgets);
      expect(find.text('Content'), findsWidgets);
      expect(find.text('Audit'), findsWidgets);
      expect(find.text('PAGE_QUESTS'), findsOneWidget);
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

    testWidgets('tapping a rail destination navigates to that route', (
      tester,
    ) async {
      await _pumpShell(
        tester,
        initialLocation: '/admin/quests',
        tokenStore: InMemoryTokenStore(),
      );

      expect(find.text('PAGE_QUESTS'), findsOneWidget);

      await tester.tap(find.text('Energy').first);
      await tester.pumpAndSettle();

      expect(find.text('PAGE_ENERGY'), findsOneWidget);
    });

    testWidgets('sign-out clears token store and routes to /admin/login', (
      tester,
    ) async {
      final store = InMemoryTokenStore();
      await store.saveAccessToken('jwt-admin');

      await _pumpShell(
        tester,
        initialLocation: '/admin/quests',
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
