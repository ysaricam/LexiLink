import 'dart:async';
import 'dart:math' as math;
import 'dart:ui';

import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/features/auth/data/guest_device_id_store.dart';
import 'package:lexilink_app/features/auth/data/guest_player_repository.dart';
import 'package:lexilink_app/features/categories/data/category.dart';
import 'package:lexilink_app/features/categories/data/category_repository.dart';
import 'package:lexilink_app/features/diamond/data/diamond_repository.dart';
import 'package:lexilink_app/features/energy/data/energy_repository.dart';
import 'package:lexilink_app/features/home/presentation/home_screen.dart';
import 'package:lexilink_app/features/settings/data/app_language.dart';
import 'package:lexilink_app/features/settings/data/locale_preferences_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';
import 'package:lexilink_app/shared/widgets/app_error_state.dart';

enum _SplashStage {
  session,
  player,
  categories,
  resources,
  ready,
}

class SplashScreen extends StatefulWidget {
  const SplashScreen({super.key});

  @override
  State<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen>
    with SingleTickerProviderStateMixin {
  static const _guestDisplayName = 'Guest Player';

  late final AnimationController _controller;
  late http.Client _httpClient;
  _SplashStage _stage = _SplashStage.session;
  Object? _error;

  @override
  void initState() {
    super.initState();
    _httpClient = http.Client();
    _controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 1500),
    )..repeat();

    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;
      unawaited(_bootstrap());
    });
  }

  @override
  void dispose() {
    _controller.dispose();
    _httpClient.close();
    super.dispose();
  }

  Future<void> _bootstrap() async {
    setState(() {
      _stage = _SplashStage.session;
      _error = null;
    });

    try {
      final backendLocale = await _resolveBackendLocale();
      final tokenStore = await SharedPreferencesTokenStore.create();
      final accessToken = await tokenStore.readAccessToken();
      final playerId = await tokenStore.readPlayerId();
      final hasExistingSession =
          (accessToken != null && accessToken.isNotEmpty) ||
          (playerId != null && playerId.isNotEmpty);
      final guestDeviceId = await GuestDeviceIdStore().readOrCreate(
        preferLegacyDeviceId: hasExistingSession,
      );

      final apiClient = ApiClient(
        config: ApiConfig.local(),
        httpClient: _httpClient,
        tokenStore: tokenStore,
      );

      var resolvedAccessToken = accessToken;
      if (resolvedAccessToken == null || resolvedAccessToken.isEmpty) {
        _setStage(_SplashStage.player);
        final session =
            await GuestPlayerRepository(
              apiClient: apiClient,
            ).registerGuest(
              deviceId: guestDeviceId,
              displayName: _guestDisplayName,
              locale: backendLocale,
            );
        await tokenStore.saveAccessToken(session.accessToken);
        await tokenStore.savePlayerId(session.playerId);
        resolvedAccessToken = session.accessToken;
      }

      _setStage(_SplashStage.categories);
      final categories = await _loadNonEmptyCategories(
        CategoryRepository(apiClient: apiClient),
        preferredLocale: backendLocale,
      );

      _setStage(_SplashStage.resources);
      final (energy, diamond) = await (
        _retry(() => EnergyRepository(apiClient: apiClient).getMe()),
        _retry(() => DiamondRepository(apiClient: apiClient).getMe()),
      ).wait;

      _setStage(_SplashStage.ready);
      await Future<void>.delayed(const Duration(milliseconds: 280));
      if (!mounted) return;

      context.go(
        '/home',
        extra: HomeInitialData(
          tokenStore: tokenStore,
          guestDeviceId: guestDeviceId,
          accessToken: resolvedAccessToken,
          categories: categories,
          energy: energy,
          diamond: diamond,
        ),
      );
    } on Object catch (error) {
      if (!mounted) return;
      setState(() => _error = error);
    }
  }

  Future<String> _resolveBackendLocale() async {
    AppLanguage? stored;
    try {
      stored = await SharedPreferencesLocalePreferencesRepository().load();
    } on Object {
      stored = null;
    }
    final device =
        AppLanguage.fromCode(PlatformDispatcher.instance.locale.languageCode) ??
        AppLanguage.fallback;
    return (stored ?? device).backendLocale;
  }

  Future<List<Category>> _loadNonEmptyCategories(
    CategoryRepository repository, {
    required String preferredLocale,
  }) async {
    final locales = <String>{
      preferredLocale,
      AppLanguage.turkish.backendLocale,
      AppLanguage.english.backendLocale,
    };

    for (final locale in locales) {
      final categories = await _retry(
        () => repository.getCategories(locale: locale),
      );
      if (categories.isNotEmpty) {
        return categories;
      }
    }

    throw StateError('No playable categories are available.');
  }

  void _setStage(_SplashStage stage) {
    if (!mounted) return;
    setState(() => _stage = stage);
  }

  Future<T> _retry<T>(Future<T> Function() request) async {
    Object? lastError;
    for (var attempt = 0; attempt < 4; attempt++) {
      try {
        return await request();
      } on Object catch (error) {
        lastError = error;
        await Future<void>.delayed(Duration(milliseconds: 350 * (attempt + 1)));
      }
    }
    Error.throwWithStackTrace(lastError!, StackTrace.current);
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      backgroundColor: theme.scaffoldBackgroundColor,
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(24, 28, 24, 32),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const Spacer(flex: 2),
              const _SplashLogo(),
              const SizedBox(height: 34),
              if (_error == null)
                _StepLoader(
                  animation: _controller,
                  stage: _stage,
                )
              else
                _SplashRetry(error: _error!, onRetry: _bootstrap),
              const Spacer(flex: 3),
            ],
          ),
        ),
      ),
    );
  }
}

class _SplashLogo extends StatelessWidget {
  const _SplashLogo();

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Text(
          context.l10n.appTitle,
          textAlign: TextAlign.center,
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
          style: theme.textTheme.displaySmall?.copyWith(
            color: theme.colorScheme.secondary,
            fontWeight: FontWeight.w900,
            height: 1,
            letterSpacing: 0,
          ),
        ),
        const SizedBox(height: 10),
        Container(
          width: 76,
          height: 4,
          decoration: BoxDecoration(
            color: theme.colorScheme.primary.withValues(alpha: 0.28),
            borderRadius: BorderRadius.circular(999),
          ),
        ),
      ],
    );
  }
}

class _StepLoader extends StatelessWidget {
  const _StepLoader({
    required this.animation,
    required this.stage,
  });

  final Animation<double> animation;
  final _SplashStage stage;

  @override
  Widget build(BuildContext context) {
    final labels = _labels(context);
    final activeIndex = stage.index.clamp(0, labels.length - 1);
    final theme = Theme.of(context);

    return AnimatedBuilder(
      animation: animation,
      builder: (context, _) {
        return Column(
          children: [
            SizedBox(
              height: 42,
              child: CustomPaint(
                painter: _WordPathPainter(
                  progress: animation.value,
                  activeIndex: activeIndex,
                  nodeCount: labels.length,
                  primary: theme.colorScheme.primary,
                  secondary: theme.colorScheme.secondary,
                  muted: theme.colorScheme.outline.withValues(alpha: 0.26),
                ),
                child: const SizedBox.expand(),
              ),
            ),
            const SizedBox(height: 16),
            AnimatedSwitcher(
              duration: const Duration(milliseconds: 180),
              child: Text(
                labels[activeIndex],
                key: ValueKey(labels[activeIndex]),
                textAlign: TextAlign.center,
                style: theme.textTheme.titleSmall?.copyWith(
                  color: theme.colorScheme.onSurface,
                  fontWeight: FontWeight.w800,
                ),
              ),
            ),
            const SizedBox(height: 8),
            Text(
              context.l10n.splashLoadingSubtitle,
              textAlign: TextAlign.center,
              style: theme.textTheme.bodySmall?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
                height: 1.35,
              ),
            ),
          ],
        );
      },
    );
  }

  List<String> _labels(BuildContext context) {
    final l10n = context.l10n;
    return [
      l10n.splashStageSession,
      l10n.splashStagePlayer,
      l10n.splashStageCategories,
      l10n.splashStageResources,
      l10n.splashStageReady,
    ];
  }
}

class _WordPathPainter extends CustomPainter {
  _WordPathPainter({
    required this.progress,
    required this.activeIndex,
    required this.nodeCount,
    required this.primary,
    required this.secondary,
    required this.muted,
  });

  final double progress;
  final int activeIndex;
  final int nodeCount;
  final Color primary;
  final Color secondary;
  final Color muted;

  @override
  void paint(Canvas canvas, Size size) {
    if (nodeCount < 2) return;

    final left = size.width * 0.12;
    final right = size.width * 0.88;
    final centerY = size.height / 2;
    final gap = (right - left) / (nodeCount - 1);
    final points = [
      for (var i = 0; i < nodeCount; i++)
        Offset(left + gap * i, centerY + math.sin(i * 1.7) * 5),
    ];

    final linePaint = Paint()
      ..color = muted
      ..strokeWidth = 3
      ..strokeCap = StrokeCap.round;
    for (var i = 0; i < points.length - 1; i++) {
      canvas.drawLine(points[i], points[i + 1], linePaint);
    }

    final completePaint = Paint()
      ..color = primary
      ..strokeWidth = 3
      ..strokeCap = StrokeCap.round;
    for (var i = 0; i < activeIndex; i++) {
      canvas.drawLine(points[i], points[i + 1], completePaint);
    }

    final pulse = Curves.easeInOut.transform(
      math.sin(progress * math.pi).abs(),
    );
    for (var i = 0; i < points.length; i++) {
      final isComplete = i < activeIndex;
      final isActive = i == activeIndex;
      final radius = isActive ? 6.5 + pulse * 3 : 5.5;
      final fill = isActive ? secondary : (isComplete ? primary : muted);
      final nodePaint = Paint()..color = fill;
      canvas
        ..drawCircle(
          points[i],
          radius + 3,
          Paint()..color = fill.withValues(alpha: 0.12),
        )
        ..drawCircle(points[i], radius, nodePaint);
    }

    final start = points[activeIndex];
    final end = activeIndex >= points.length - 1
        ? points[activeIndex]
        : points[activeIndex + 1];
    final travelerT = Curves.easeInOut.transform(progress);
    final traveler = Offset.lerp(start, end, travelerT)!;
    canvas.drawCircle(
      traveler,
      3.2,
      Paint()..color = secondary.withValues(alpha: 0.86),
    );
  }

  @override
  bool shouldRepaint(_WordPathPainter oldDelegate) {
    return oldDelegate.progress != progress ||
        oldDelegate.activeIndex != activeIndex ||
        oldDelegate.nodeCount != nodeCount ||
        oldDelegate.primary != primary ||
        oldDelegate.secondary != secondary ||
        oldDelegate.muted != muted;
  }
}

class _SplashRetry extends StatelessWidget {
  const _SplashRetry({
    required this.error,
    required this.onRetry,
  });

  final Object error;
  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) {
    return AppErrorState(
      title: context.l10n.splashFailedTitle,
      message: context.l10n.splashFailedMessage,
      retryLabel: context.l10n.commonRetry,
      onRetry: onRetry,
    );
  }
}
