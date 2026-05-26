import 'dart:async';

import 'package:flutter/gestures.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/app/theme/app_palette.dart';
import 'package:lexilink_app/features/auth/application/guest_entry_cubit.dart';
import 'package:lexilink_app/features/auth/data/guest_player_repository.dart';
import 'package:lexilink_app/features/categories/application/category_list_cubit.dart';
import 'package:lexilink_app/features/categories/data/category.dart';
import 'package:lexilink_app/features/categories/data/category_repository.dart';
import 'package:lexilink_app/features/energy/application/energy_cubit.dart';
import 'package:lexilink_app/features/energy/data/energy_repository.dart';
import 'package:lexilink_app/features/energy/presentation/energy_badge.dart';
import 'package:lexilink_app/features/hint/application/hint_cubit.dart';
import 'package:lexilink_app/features/hint/data/hint_repository.dart';
import 'package:lexilink_app/features/hint/presentation/hint_badge.dart';
import 'package:lexilink_app/features/undo/application/undo_cubit.dart';
import 'package:lexilink_app/features/undo/data/undo_repository.dart';
import 'package:lexilink_app/features/undo/presentation/undo_badge.dart';
import 'package:lexilink_app/features/reset/application/reset_cubit.dart';
import 'package:lexilink_app/features/reset/data/reset_repository.dart';
import 'package:lexilink_app/features/reset/presentation/reset_badge.dart';
import 'package:lexilink_app/features/game/application/game_start_cubit.dart';
import 'package:lexilink_app/features/game/data/game_repository.dart';
import 'package:lexilink_app/features/session/application/session_cubit.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';
import 'package:lexilink_app/shared/widgets/app_button.dart';
import 'package:lexilink_app/shared/widgets/app_error_state.dart';
import 'package:lexilink_app/shared/widgets/app_loading_state.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  late final Future<SharedPreferencesTokenStore> _tokenStoreFuture;

  @override
  void initState() {
    super.initState();
    _tokenStoreFuture = SharedPreferencesTokenStore.create();
  }

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<TokenStore>(
      future: _tokenStoreFuture,
      builder: (context, snapshot) {
        if (snapshot.hasError) {
          return const Scaffold(
            body: SafeArea(
              child: Center(
                child: AppErrorState(
                  title: 'Session storage failed',
                  message: 'Restart the app and try again.',
                ),
              ),
            ),
          );
        }

        final tokenStore = snapshot.data;
        if (tokenStore == null) {
          return const Scaffold(
            body: SafeArea(
              child: Center(
                child: AppLoadingState(message: 'Preparing session...'),
              ),
            ),
          );
        }

        return _HomeProviders(tokenStore: tokenStore);
      },
    );
  }
}

class _HomeProviders extends StatefulWidget {
  const _HomeProviders({required this.tokenStore});

  final TokenStore tokenStore;

  @override
  State<_HomeProviders> createState() => _HomeProvidersState();
}

class _HomeProvidersState extends State<_HomeProviders> {
  late final http.Client _httpClient;
  late final SessionCubit _sessionCubit;
  late final GuestEntryCubit _guestEntryCubit;
  late final CategoryListCubit _categoryListCubit;
  late final EnergyCubit _energyCubit;
  late final HintCubit _hintCubit;
  late final UndoCubit _undoCubit;
  late final ResetCubit _resetCubit;
  late final GameStartCubit _gameStartCubit;

  @override
  void initState() {
    super.initState();
    _httpClient = http.Client();
    final apiClient = ApiClient(
      config: ApiConfig.local(),
      httpClient: _httpClient,
      tokenStore: widget.tokenStore,
    );

    _sessionCubit = SessionCubit(tokenStore: widget.tokenStore);
    _guestEntryCubit = GuestEntryCubit(
      guestPlayerRepository: GuestPlayerRepository(apiClient: apiClient),
      sessionCubit: _sessionCubit,
    );
    _categoryListCubit = CategoryListCubit(
      categoryRepository: CategoryRepository(apiClient: apiClient),
    );
    _energyCubit = EnergyCubit(
      energyRepository: EnergyRepository(apiClient: apiClient),
    );
    _hintCubit = HintCubit(
      hintRepository: HintRepository(apiClient: apiClient),
    );
    _undoCubit = UndoCubit(
      undoRepository: UndoRepository(apiClient: apiClient),
    );
    _resetCubit = ResetCubit(
      resetRepository: ResetRepository(apiClient: apiClient),
    );
    _gameStartCubit = GameStartCubit(
      gameRepository: GameRepository(apiClient: apiClient),
      tokenStore: widget.tokenStore,
    );

    unawaited(_sessionCubit.checkSession());
  }

  @override
  void dispose() {
    _gameStartCubit.close();
    _resetCubit.close();
    _undoCubit.close();
    _hintCubit.close();
    _energyCubit.close();
    _categoryListCubit.close();
    _guestEntryCubit.close();
    _sessionCubit.close();
    _httpClient.close();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return MultiBlocProvider(
      providers: [
        BlocProvider.value(value: _sessionCubit),
        BlocProvider.value(value: _guestEntryCubit),
        BlocProvider.value(value: _categoryListCubit),
        BlocProvider.value(value: _energyCubit),
        BlocProvider.value(value: _hintCubit),
        BlocProvider.value(value: _undoCubit),
        BlocProvider.value(value: _resetCubit),
        BlocProvider.value(value: _gameStartCubit),
      ],
      child: const _HomeView(),
    );
  }
}

class _HomeView extends StatelessWidget {
  const _HomeView();

  static const _guestDeviceId = 'frontend-preview-device';
  static const _guestDisplayName = 'Guest Player';
  static const _guestLocale = 'en-US';

  @override
  Widget build(BuildContext context) {
    return MultiBlocListener(
      listeners: [
        BlocListener<SessionCubit, SessionState>(
          listenWhen: (previous, current) =>
              previous.status != current.status,
          listener: (context, state) {
            if (state.status == SessionStatus.unauthenticated) {
              context.read<GuestEntryCubit>().continueAsGuest(
                deviceId: _guestDeviceId,
                displayName: _guestDisplayName,
                locale: _guestLocale,
              );
            } else if (state.status == SessionStatus.authenticated) {
              context.read<CategoryListCubit>().loadCategories();
              context.read<EnergyCubit>().loadEnergy();
              context.read<HintCubit>().loadHint();
              context.read<UndoCubit>().loadUndo();
              context.read<ResetCubit>().loadReset();
            }
          },
        ),
        BlocListener<GameStartCubit, GameStartState>(
          listenWhen: (previous, current) =>
              previous.status != current.status,
          listener: (context, state) {
            if (state.status == GameStartStatus.success &&
                state.gameId != null) {
              context.go('/games/${state.gameId}');
            } else if (state.status == GameStartStatus.failure) {
              context.read<EnergyCubit>().loadEnergy();
            }
          },
        ),
      ],
      child: const _HomeScaffold(),
    );
  }
}

class _HomeScaffold extends StatelessWidget {
  const _HomeScaffold();

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      backgroundColor: theme.scaffoldBackgroundColor,
      body: SafeArea(
        child: Stack(
          children: [
            const Positioned.fill(
              child: Padding(
                padding: EdgeInsets.fromLTRB(76, 64, 16, 24),
                child: _HomeContent(),
              ),
            ),
            const Positioned(
              top: 12,
              right: 12,
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  ResetBadge(),
                  SizedBox(width: 8),
                  UndoBadge(),
                  SizedBox(width: 8),
                  HintBadge(),
                  SizedBox(width: 8),
                  EnergyBadge(),
                ],
              ),
            ),
            Positioned(
              top: 12,
              left: 12,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  _SideIconButton(
                    icon: Icons.person_outline,
                    tooltip: 'Profile',
                    onPressed: () => context.go('/profile'),
                  ),
                  const SizedBox(height: 10),
                  _SideIconButton(
                    icon: Icons.flag_outlined,
                    tooltip: 'Quests',
                    onPressed: () => context.go('/quests'),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _HomeContent extends StatelessWidget {
  const _HomeContent();

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<CategoryListCubit, CategoryListState>(
      builder: (context, state) {
        if (state.status == CategoryListStatus.initial || state.isLoading) {
          return const Center(
            child: AppLoadingState(message: 'Loading categories...'),
          );
        }

        if (state.status == CategoryListStatus.failure) {
          return Center(
            child: AppErrorState(
              title: 'Could not load categories',
              message: state.message ?? 'Try again.',
              onRetry: () =>
                  context.read<CategoryListCubit>().loadCategories(),
            ),
          );
        }

        if (state.categories.isEmpty) {
          return const Center(
            child: AppLoadingState(message: 'Preparing categories...'),
          );
        }

        return _CategoryDeck(categories: state.categories);
      },
    );
  }
}

class _CategoryDeck extends StatefulWidget {
  const _CategoryDeck({required this.categories});

  final List<Category> categories;

  @override
  State<_CategoryDeck> createState() => _CategoryDeckState();
}

class _CategoryDeckState extends State<_CategoryDeck> {
  late final PageController _controller;
  int _currentIndex = 0;

  @override
  void initState() {
    super.initState();
    _controller = PageController(viewportFraction: 0.82);
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted || widget.categories.isEmpty) return;
      context
          .read<CategoryListCubit>()
          .selectCategory(widget.categories[_currentIndex].id);
    });
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _onPageChanged(int index) {
    setState(() => _currentIndex = index);
    context
        .read<CategoryListCubit>()
        .selectCategory(widget.categories[index].id);
  }

  @override
  Widget build(BuildContext context) {
    final gameStartState = context.watch<GameStartCubit>().state;

    return Align(
      alignment: const Alignment(0, -0.18),
      child: ConstrainedBox(
        constraints: const BoxConstraints(maxWidth: 420),
        child: LayoutBuilder(
          builder: (context, constraints) {
            final cardSize = constraints.maxWidth * 0.82;
            return Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                SizedBox(
                  width: cardSize * 0.72,
                  child: const FittedBox(
                    fit: BoxFit.fitWidth,
                    child: _LexiLinkWordmark(),
                  ),
                ),
                const SizedBox(height: 18),
                SizedBox(
                  height: cardSize,
                  child: ScrollConfiguration(
                    behavior: const _DragScrollBehavior(),
                    child: PageView.builder(
                      controller: _controller,
                      itemCount: widget.categories.length,
                      onPageChanged: _onPageChanged,
                      itemBuilder: (context, index) {
                        return AnimatedBuilder(
                          animation: _controller,
                          builder: (context, child) {
                            var offsetFromCenter = 0.0;
                            if (_controller.position.haveDimensions) {
                              final page = _controller.page ??
                                  _controller.initialPage.toDouble();
                              offsetFromCenter = page - index;
                            } else {
                              offsetFromCenter =
                                  (_currentIndex - index).toDouble();
                            }
                            final distance =
                                offsetFromCenter.abs().clamp(0.0, 1.0);
                            final scale = 1 - distance * 0.06;
                            return Transform.scale(
                              scale: scale,
                              child: child,
                            );
                          },
                          child: _CategoryCard(
                            category: widget.categories[index],
                          ),
                        );
                      },
                    ),
                  ),
                ),
                const SizedBox(height: 16),
                _PageDots(
                  count: widget.categories.length,
                  current: _currentIndex,
                ),
                const SizedBox(height: 24),
                SizedBox(
                  width: cardSize,
                  child: AppPrimaryButton(
                    label:
                        gameStartState.isSubmitting ? 'Starting...' : 'Start',
                    onPressed: gameStartState.isSubmitting
                        ? null
                        : () => context.read<GameStartCubit>().startGame(
                            categoryId: widget.categories[_currentIndex].id,
                          ),
                  ),
                ),
                if (gameStartState.status == GameStartStatus.failure) ...[
                  const SizedBox(height: 12),
                  AppErrorState(
                    title: 'Could not start game',
                    message: gameStartState.message ?? 'Try again.',
                  ),
                ],
              ],
            );
          },
        ),
      ),
    );
  }
}

class _LexiLinkWordmark extends StatelessWidget {
  const _LexiLinkWordmark();

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Text(
      'LexiLink',
      textAlign: TextAlign.center,
      style: theme.textTheme.headlineMedium?.copyWith(
        color: AppPalette.focus,
        letterSpacing: 1.2,
        fontWeight: FontWeight.w700,
      ),
    );
  }
}

class _DragScrollBehavior extends MaterialScrollBehavior {
  const _DragScrollBehavior();

  @override
  Set<PointerDeviceKind> get dragDevices => {
        PointerDeviceKind.touch,
        PointerDeviceKind.mouse,
        PointerDeviceKind.stylus,
        PointerDeviceKind.trackpad,
      };
}

class _CategoryCard extends StatelessWidget {
  const _CategoryCard({required this.category});

  final Category category;

  @override
  Widget build(BuildContext context) {
    final visuals = _categoryVisuals(category.name);

    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 6),
      child: DecoratedBox(
        decoration: BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
            colors: visuals.gradient,
          ),
          borderRadius: BorderRadius.circular(20),
          boxShadow: [
            BoxShadow(
              color: visuals.gradient.last.withValues(alpha: 0.22),
              blurRadius: 14,
              offset: const Offset(0, 8),
            ),
          ],
        ),
        child: Padding(
          padding: const EdgeInsets.all(18),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                category.name,
                style: Theme.of(context).textTheme.titleLarge?.copyWith(
                  color: Colors.white,
                  fontWeight: FontWeight.w700,
                ),
              ),
              const Spacer(),
              Center(
                child: Text(
                  visuals.emoji,
                  style: const TextStyle(fontSize: 72, height: 1),
                ),
              ),
              const Spacer(),
            ],
          ),
        ),
      ),
    );
  }
}

class _PageDots extends StatelessWidget {
  const _PageDots({required this.count, required this.current});

  final int count;
  final int current;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;
    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        for (var i = 0; i < count; i++)
          AnimatedContainer(
            duration: const Duration(milliseconds: 200),
            margin: const EdgeInsets.symmetric(horizontal: 4),
            height: 8,
            width: i == current ? 22 : 8,
            decoration: BoxDecoration(
              color: i == current
                  ? colorScheme.primary
                  : colorScheme.outline.withValues(alpha: 0.4),
              borderRadius: BorderRadius.circular(99),
            ),
          ),
      ],
    );
  }
}

class _SideIconButton extends StatelessWidget {
  const _SideIconButton({
    required this.icon,
    required this.tooltip,
    required this.onPressed,
  });

  final IconData icon;
  final String tooltip;
  final VoidCallback onPressed;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;
    return Material(
      color: colorScheme.surface,
      shape: const CircleBorder(),
      elevation: 1,
      child: InkWell(
        customBorder: const CircleBorder(),
        onTap: onPressed,
        child: Tooltip(
          message: tooltip,
          child: Padding(
            padding: const EdgeInsets.all(10),
            child: Icon(icon, color: colorScheme.primary),
          ),
        ),
      ),
    );
  }
}

class _CategoryVisual {
  const _CategoryVisual({required this.emoji, required this.gradient});

  final String emoji;
  final List<Color> gradient;
}

_CategoryVisual _categoryVisuals(String name) {
  final lower = name.toLowerCase();
  if (lower.contains('hayv')) {
    return const _CategoryVisual(
      emoji: '🦊',
      gradient: [Color(0xff3d7c5a), Color(0xff1f5040)],
    );
  }
  if (lower.contains('yem') || lower.contains('food')) {
    return const _CategoryVisual(
      emoji: '🍜',
      gradient: [Color(0xffe39d4a), Color(0xffb04d2b)],
    );
  }
  if (lower.contains('doğa') ||
      lower.contains('doga') ||
      lower.contains('nature')) {
    return const _CategoryVisual(
      emoji: '🌿',
      gradient: [Color(0xff2f7e93), Color(0xff124559)],
    );
  }
  return const _CategoryVisual(
    emoji: '🎲',
    gradient: [AppPalette.primary, AppPalette.primaryPressed],
  );
}
