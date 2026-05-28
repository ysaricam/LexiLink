import 'dart:async';

import 'package:flutter/widgets.dart';
import 'package:go_router/go_router.dart';
import 'package:lexilink_app/features/admin/presentation/admin_placeholder_page.dart';
import 'package:lexilink_app/features/admin/presentation/app_admin_shell.dart';
import 'package:lexilink_app/features/admin_auth/data/admin_token_store.dart';
import 'package:lexilink_app/features/admin_auth/presentation/admin_login_screen.dart';
import 'package:lexilink_app/features/admin_market/presentation/admin_market_screen.dart';
import 'package:lexilink_app/features/categories/presentation/category_selection_screen.dart';
import 'package:lexilink_app/features/game/presentation/game_screen.dart';
import 'package:lexilink_app/features/home/presentation/home_screen.dart';
import 'package:lexilink_app/features/market/presentation/market_screen.dart';
import 'package:lexilink_app/features/payments/presentation/payment_screen.dart';
import 'package:lexilink_app/features/profile/presentation/leaderboard_screen.dart';
import 'package:lexilink_app/features/profile/presentation/profile_summary_screen.dart';
import 'package:lexilink_app/features/quests/presentation/quests_screen.dart';
import 'package:lexilink_app/features/splash/presentation/splash_screen.dart';

final appRouter = GoRouter(
  redirect: _adminAuthGuard,
  routes: [
    GoRoute(
      path: '/',
      builder: (context, state) => const SplashScreen(),
    ),
    GoRoute(
      path: '/home',
      builder: (context, state) => const HomeScreen(),
    ),
    GoRoute(
      path: '/categories',
      builder: (context, state) => const CategorySelectionScreen(),
    ),
    GoRoute(
      path: '/games/:gameId',
      builder: (context, state) =>
          GameScreen(gameId: state.pathParameters['gameId']!),
    ),
    GoRoute(
      path: '/profile',
      builder: (context, state) => const ProfileSummaryScreen(),
    ),
    GoRoute(
      path: '/leaderboard',
      builder: (context, state) => const LeaderboardScreen(),
    ),
    GoRoute(
      path: '/quests',
      builder: (context, state) => const QuestsScreen(),
    ),
    GoRoute(
      path: '/market',
      builder: (context, state) => const MarketScreen(),
    ),
    GoRoute(
      path: '/payments',
      builder: (context, state) => const PaymentScreen(),
    ),
    GoRoute(
      path: '/admin/login',
      builder: (context, state) => const AdminLoginScreen(),
    ),
    // /admin → /admin/quests (default destination)
    GoRoute(
      path: '/admin',
      redirect: (_, _) => '/admin/quests',
    ),
    ShellRoute(
      builder: (context, state, child) => AppAdminShell(
        location: state.uri.toString(),
        child: child,
      ),
      routes: [
        GoRoute(
          path: '/admin/quests',
          builder: (context, state) => const AdminQuestsPage(),
        ),
        GoRoute(
          path: '/admin/players',
          builder: (context, state) => const AdminPlayersPage(),
        ),
        GoRoute(
          path: '/admin/energy',
          builder: (context, state) => const AdminEnergyPage(),
        ),
        GoRoute(
          path: '/admin/hint',
          builder: (context, state) => const AdminHintPage(),
        ),
        GoRoute(
          path: '/admin/undo',
          builder: (context, state) => const AdminUndoPage(),
        ),
        GoRoute(
          path: '/admin/reset',
          builder: (context, state) => const AdminResetPage(),
        ),
        GoRoute(
          path: '/admin/diamond',
          builder: (context, state) => const AdminDiamondPage(),
        ),
        GoRoute(
          path: '/admin/market',
          redirect: (_, _) => '/admin/market/categories',
        ),
        GoRoute(
          path: '/admin/market/categories',
          builder: (context, state) => const AdminMarketPage(),
        ),
        GoRoute(
          path: '/admin/market/items',
          builder: (context, state) => const AdminMarketPage(
            initialTab: AdminMarketTab.items,
          ),
        ),
        GoRoute(
          path: '/admin/market/orders',
          builder: (context, state) => const AdminMarketPage(
            initialTab: AdminMarketTab.orders,
          ),
        ),
        GoRoute(
          path: '/admin/market/orders/:playerId',
          builder: (context, state) => AdminMarketPage(
            initialTab: AdminMarketTab.orders,
            initialPlayerId: state.pathParameters['playerId'],
          ),
        ),
        GoRoute(
          path: '/admin/audit',
          builder: (context, state) => const AdminAuditPage(),
        ),
      ],
    ),
  ],
);

/// Router-level guard for /admin/* paths. Non-admin routes return
/// synchronously so the player-facing splash/home stack is not stalled
/// by an awaited Future on every navigation. Admin destinations
/// asynchronously consult the persisted token store and redirect to
/// /admin/login when no token exists. The login screen itself is exempt.
FutureOr<String?> _adminAuthGuard(
  BuildContext context,
  GoRouterState state,
) {
  final path = state.uri.path;
  if (!path.startsWith('/admin')) return null;
  if (path == '/admin/login') return null;
  return _redirectIfNoAdminToken();
}

Future<String?> _redirectIfNoAdminToken() async {
  final store = await SharedPreferencesAdminTokenStore.create();
  final token = await store.readAccessToken();
  if (token == null || token.isEmpty) {
    return '/admin/login';
  }
  return null;
}
