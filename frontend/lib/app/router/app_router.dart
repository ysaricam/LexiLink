import 'package:go_router/go_router.dart';
import 'package:lexilink_app/features/admin_auth/presentation/admin_home_screen.dart';
import 'package:lexilink_app/features/admin_auth/presentation/admin_login_screen.dart';
import 'package:lexilink_app/features/categories/presentation/category_selection_screen.dart';
import 'package:lexilink_app/features/game/presentation/game_screen.dart';
import 'package:lexilink_app/features/home/presentation/home_screen.dart';
import 'package:lexilink_app/features/profile/presentation/leaderboard_screen.dart';
import 'package:lexilink_app/features/profile/presentation/profile_summary_screen.dart';
import 'package:lexilink_app/features/quests/presentation/quests_screen.dart';
import 'package:lexilink_app/features/splash/presentation/splash_screen.dart';

final appRouter = GoRouter(
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
      path: '/admin/login',
      builder: (context, state) => const AdminLoginScreen(),
    ),
    GoRoute(
      path: '/admin',
      builder: (context, state) => const AdminHomeScreen(),
    ),
  ],
);
