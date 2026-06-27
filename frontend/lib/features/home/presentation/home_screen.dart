import 'dart:async';

import 'package:flutter/gestures.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/features/auth/application/guest_entry_cubit.dart';
import 'package:lexilink_app/features/auth/data/guest_device_id_store.dart';
import 'package:lexilink_app/features/auth/data/guest_player_repository.dart';
import 'package:lexilink_app/features/categories/application/category_list_cubit.dart';
import 'package:lexilink_app/features/categories/data/category.dart';
import 'package:lexilink_app/features/categories/data/category_repository.dart';
import 'package:lexilink_app/features/diamond/application/diamond_cubit.dart';
import 'package:lexilink_app/features/diamond/data/diamond_repository.dart';
import 'package:lexilink_app/features/diamond/data/player_diamond.dart';
import 'package:lexilink_app/features/diamond/presentation/diamond_badge.dart';
import 'package:lexilink_app/features/energy/application/energy_cubit.dart';
import 'package:lexilink_app/features/energy/data/energy_repository.dart';
import 'package:lexilink_app/features/energy/data/player_energy.dart';
import 'package:lexilink_app/features/energy/presentation/energy_badge.dart';
import 'package:lexilink_app/features/game/application/game_start_cubit.dart';
import 'package:lexilink_app/features/game/data/game_repository.dart';
import 'package:lexilink_app/features/market/data/market_models.dart';
import 'package:lexilink_app/features/market/presentation/market_screen.dart';
import 'package:lexilink_app/features/session/application/session_cubit.dart';
import 'package:lexilink_app/features/settings/application/locale_cubit.dart';
import 'package:lexilink_app/shared/ads/ad_config.dart';
import 'package:lexilink_app/shared/ads/ads_service.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/audio/audio_service.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';
import 'package:lexilink_app/shared/widgets/app_button.dart';
import 'package:lexilink_app/shared/widgets/app_error_state.dart';
import 'package:lexilink_app/shared/widgets/app_loading_state.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({
    this.initialData,
    super.key,
  });

  final HomeInitialData? initialData;

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  late final Future<_HomeBootstrap> _bootstrapFuture;

  @override
  void initState() {
    super.initState();
    _bootstrapFuture = widget.initialData == null
        ? _createBootstrap()
        : Future.value(_HomeBootstrap.fromInitialData(widget.initialData!));
  }

  Future<_HomeBootstrap> _createBootstrap() async {
    final tokenStore = await SharedPreferencesTokenStore.create();
    final accessToken = await tokenStore.readAccessToken();
    final playerId = await tokenStore.readPlayerId();
    final hasExistingSession =
        (accessToken != null && accessToken.isNotEmpty) ||
        (playerId != null && playerId.isNotEmpty);
    final guestDeviceId = await GuestDeviceIdStore().readOrCreate(
      preferLegacyDeviceId: hasExistingSession,
    );

    return _HomeBootstrap(
      tokenStore: tokenStore,
      guestDeviceId: guestDeviceId,
    );
  }

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<_HomeBootstrap>(
      future: _bootstrapFuture,
      builder: (context, snapshot) {
        if (snapshot.hasError) {
          return Scaffold(
            body: SafeArea(
              child: Center(
                child: AppErrorState(
                  title: context.l10n.sessionStorageFailedTitle,
                  message: context.l10n.sessionStorageFailedMessage,
                ),
              ),
            ),
          );
        }

        final bootstrap = snapshot.data;
        if (bootstrap == null) {
          return Scaffold(
            body: SafeArea(
              child: Center(
                child: AppLoadingState(message: context.l10n.preparingSession),
              ),
            ),
          );
        }

        return _HomeProviders(bootstrap: bootstrap);
      },
    );
  }
}

class _HomeBootstrap {
  const _HomeBootstrap({
    required this.tokenStore,
    required this.guestDeviceId,
    this.initialData,
  });

  factory _HomeBootstrap.fromInitialData(HomeInitialData initialData) {
    return _HomeBootstrap(
      tokenStore: initialData.tokenStore,
      guestDeviceId: initialData.guestDeviceId,
      initialData: initialData,
    );
  }

  final TokenStore tokenStore;
  final String guestDeviceId;
  final HomeInitialData? initialData;
}

class HomeInitialData {
  const HomeInitialData({
    required this.tokenStore,
    required this.guestDeviceId,
    required this.accessToken,
    required this.categories,
    required this.energy,
    required this.diamond,
  });

  final TokenStore tokenStore;
  final String guestDeviceId;
  final String accessToken;
  final List<Category> categories;
  final PlayerEnergy energy;
  final PlayerDiamond diamond;
}

class _HomeProviders extends StatefulWidget {
  const _HomeProviders({required this.bootstrap});

  final _HomeBootstrap bootstrap;

  @override
  State<_HomeProviders> createState() => _HomeProvidersState();
}

class _HomeProvidersState extends State<_HomeProviders> {
  late final http.Client _httpClient;
  late final SessionCubit _sessionCubit;
  late final GuestEntryCubit _guestEntryCubit;
  late final CategoryListCubit _categoryListCubit;
  late final EnergyCubit _energyCubit;
  late final DiamondCubit _diamondCubit;
  late final GameStartCubit _gameStartCubit;

  @override
  void initState() {
    super.initState();
    _httpClient = http.Client();
    final apiClient = ApiClient(
      config: ApiConfig.local(),
      httpClient: _httpClient,
      tokenStore: widget.bootstrap.tokenStore,
    );

    final initialData = widget.bootstrap.initialData;
    _sessionCubit = SessionCubit(
      tokenStore: widget.bootstrap.tokenStore,
      initialState: initialData == null
          ? const SessionState.checking()
          : SessionState.authenticated(accessToken: initialData.accessToken),
    );
    _guestEntryCubit = GuestEntryCubit(
      guestPlayerRepository: GuestPlayerRepository(apiClient: apiClient),
      sessionCubit: _sessionCubit,
    );
    _categoryListCubit = CategoryListCubit(
      categoryRepository: CategoryRepository(apiClient: apiClient),
      initialState: initialData == null
          ? const CategoryListState.initial()
          : CategoryListState.success(categories: initialData.categories),
    );
    _energyCubit = EnergyCubit(
      energyRepository: EnergyRepository(apiClient: apiClient),
      initialState: initialData == null
          ? const EnergyState.initial()
          : EnergyState.success(energy: initialData.energy),
    );
    _diamondCubit = DiamondCubit(
      diamondRepository: DiamondRepository(apiClient: apiClient),
      initialState: initialData == null
          ? const DiamondState.initial()
          : DiamondState.success(diamond: initialData.diamond),
    );
    _gameStartCubit = GameStartCubit(
      gameRepository: GameRepository(apiClient: apiClient),
      tokenStore: widget.bootstrap.tokenStore,
    );

    if (initialData == null) {
      WidgetsBinding.instance.addPostFrameCallback((_) {
        if (!mounted) return;
        unawaited(_sessionCubit.checkSession());
      });
    }
  }

  @override
  void dispose() {
    _gameStartCubit.close();
    _diamondCubit.close();
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
        BlocProvider.value(value: _diamondCubit),
        BlocProvider.value(value: _gameStartCubit),
      ],
      child: _HomeView(guestDeviceId: widget.bootstrap.guestDeviceId),
    );
  }
}

class _HomeView extends StatelessWidget {
  const _HomeView({required this.guestDeviceId});

  static const _guestDisplayName = 'Guest Player';

  final String guestDeviceId;

  @override
  Widget build(BuildContext context) {
    return MultiBlocListener(
      listeners: [
        BlocListener<SessionCubit, SessionState>(
          listenWhen: (previous, current) => previous.status != current.status,
          listener: (context, state) {
            if (state.status == SessionStatus.unauthenticated) {
              context.read<GuestEntryCubit>().continueAsGuest(
                deviceId: guestDeviceId,
                displayName: _guestDisplayName,
                locale: context.read<LocaleCubit>().state.backendLocale,
              );
            } else if (state.status == SessionStatus.authenticated) {
              context.read<CategoryListCubit>().loadCategories(
                locale: context.read<LocaleCubit>().state.backendLocale,
              );
              context.read<EnergyCubit>().loadEnergy();
              context.read<DiamondCubit>().loadDiamond();
            }
          },
        ),
        BlocListener<GameStartCubit, GameStartState>(
          listenWhen: (previous, current) => previous.status != current.status,
          listener: (context, state) {
            if (state.status == GameStartStatus.success &&
                state.gameId != null) {
              // Interstitial @ game start (~1/3). Fire-and-forget and
              // web-safe; never blocks navigation to the game.
              context.read<AdsService>().maybeShowInterstitial(
                InterstitialChance.gameStart,
              );
              context.go('/games/${state.gameId}');
            } else if (state.status == GameStartStatus.failure) {
              context.read<AudioService>().playEffect(SoundEffect.error);
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
      body: const SafeArea(
        child: Padding(
          padding: EdgeInsets.fromLTRB(16, 12, 16, 18),
          child: Column(
            children: [
              _HomeTopBar(),
              Expanded(child: _HomeContent()),
              SizedBox(height: 14),
              _HomeActionDock(),
            ],
          ),
        ),
      ),
    );
  }
}

class _HomeTopBar extends StatelessWidget {
  const _HomeTopBar();

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        _TopIconButton(
          icon: Icons.person_outline,
          tooltip: context.l10n.navProfile,
          onPressed: () => context.go('/profile'),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: Align(
            alignment: Alignment.centerRight,
            child: SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              reverse: true,
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  _TopIconButton(
                    icon: Icons.ondemand_video_outlined,
                    tooltip: context.l10n.navEarnDiamonds,
                    onPressed: () => context.go('/earn-diamonds'),
                  ),
                  const SizedBox(width: 8),
                  const DiamondBadge(),
                  const SizedBox(width: 8),
                  const EnergyBadge(compact: true),
                ],
              ),
            ),
          ),
        ),
      ],
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
          return Center(
            child: AppLoadingState(message: context.l10n.loadingCategories),
          );
        }

        if (state.status == CategoryListStatus.failure) {
          return Center(
            child: AppErrorState(
              title: context.l10n.couldNotLoadCategories,
              message: state.message ?? context.l10n.commonTryAgain,
              onRetry: () => context.read<CategoryListCubit>().loadCategories(
                locale: context.read<LocaleCubit>().state.backendLocale,
              ),
            ),
          );
        }

        if (state.categories.isEmpty) {
          return Center(
            child: AppLoadingState(message: context.l10n.preparingCategories),
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
      context.read<CategoryListCubit>().selectCategory(
        widget.categories[_currentIndex].id,
      );
    });
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _onPageChanged(int index) {
    setState(() => _currentIndex = index);
    context.read<CategoryListCubit>().selectCategory(
      widget.categories[index].id,
    );
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
                    child: _WordLopeWordmark(),
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
                              final page =
                                  _controller.page ??
                                  _controller.initialPage.toDouble();
                              offsetFromCenter = page - index;
                            } else {
                              offsetFromCenter = (_currentIndex - index)
                                  .toDouble();
                            }
                            final distance = offsetFromCenter.abs().clamp(
                              0.0,
                              1.0,
                            );
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
                    label: gameStartState.isSubmitting
                        ? context.l10n.commonStarting
                        : context.l10n.commonStart,
                    onPressed: gameStartState.isSubmitting
                        ? null
                        : () {
                            context.read<AudioService>().playEffect(
                              SoundEffect.buttonTap,
                            );
                            _startGameOrOpenEnergyMarket(
                              context,
                              widget.categories[_currentIndex].id,
                            );
                          },
                  ),
                ),
                if (gameStartState.status == GameStartStatus.failure) ...[
                  const SizedBox(height: 12),
                  AppErrorState(
                    title: context.l10n.couldNotStartGame,
                    message:
                        gameStartState.message ?? context.l10n.commonTryAgain,
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

Future<void> _startGameOrOpenEnergyMarket(
  BuildContext context,
  String categoryId,
) async {
  final energyCubit = context.read<EnergyCubit>();
  final diamondCubit = context.read<DiamondCubit>();
  final gameStartCubit = context.read<GameStartCubit>();
  final energy = energyCubit.state.energy;
  if (energy != null && energy.currentAmount <= 0) {
    await showMarketSheet(context, initialItemType: MarketItemType.energy);
    await energyCubit.loadEnergy();
    await diamondCubit.loadDiamond();
    return;
  }

  await gameStartCubit.startGame(categoryId: categoryId);
}

class _WordLopeWordmark extends StatelessWidget {
  const _WordLopeWordmark();

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Text(
      'WordLope',
      textAlign: TextAlign.center,
      style: theme.textTheme.headlineMedium?.copyWith(
        color: theme.colorScheme.secondary,
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
    final colorScheme = Theme.of(context).colorScheme;
    final visuals = _categoryVisuals(category.name, colorScheme);

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

class _HomeActionDock extends StatelessWidget {
  const _HomeActionDock();

  @override
  Widget build(BuildContext context) {
    final actions = [
      _HomeAction(
        icon: Icons.flag_outlined,
        label: context.l10n.navQuests,
        route: '/quests',
      ),
      _HomeAction(
        icon: Icons.storefront_outlined,
        label: context.l10n.navMarket,
        route: '/market',
      ),
      _HomeAction(
        icon: Icons.diamond_outlined,
        label: context.l10n.navDiamonds,
        route: '/payments',
      ),
      _HomeAction(
        icon: Icons.settings_outlined,
        label: context.l10n.navSettings,
        route: '/settings',
      ),
    ];

    return DecoratedBox(
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.surface,
        border: Border.all(
          color: Theme.of(context).colorScheme.outline.withValues(alpha: 0.22),
        ),
        borderRadius: BorderRadius.circular(18),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.05),
            blurRadius: 12,
            offset: const Offset(0, 6),
          ),
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 10),
        child: Row(
          children: [
            for (final action in actions)
              Expanded(child: _HomeDockButton(action: action)),
          ],
        ),
      ),
    );
  }
}

class _HomeAction {
  const _HomeAction({
    required this.icon,
    required this.label,
    required this.route,
  });

  final IconData icon;
  final String label;
  final String route;
}

class _HomeDockButton extends StatelessWidget {
  const _HomeDockButton({required this.action});

  final _HomeAction action;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;
    final textTheme = Theme.of(context).textTheme;

    return Tooltip(
      message: action.label,
      child: InkWell(
        borderRadius: BorderRadius.circular(14),
        onTap: () {
          context.read<AudioService>().playEffect(SoundEffect.buttonTap);
          context.go(action.route);
        },
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 4, vertical: 4),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(action.icon, color: colorScheme.primary, size: 22),
              const SizedBox(height: 5),
              Text(
                action.label,
                style: textTheme.labelSmall?.copyWith(
                  color: colorScheme.onSurfaceVariant,
                  fontWeight: FontWeight.w600,
                ),
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                textAlign: TextAlign.center,
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _TopIconButton extends StatelessWidget {
  const _TopIconButton({
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
        onTap: () {
          context.read<AudioService>().playEffect(SoundEffect.buttonTap);
          onPressed();
        },
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

_CategoryVisual _categoryVisuals(String name, ColorScheme colorScheme) {
  final lower = name.toLowerCase();
  if (lower.contains('hayv')) {
    return _CategoryVisual(
      emoji: '🦊',
      gradient: [
        colorScheme.primary,
        Color.lerp(colorScheme.primary, Colors.black, 0.36)!,
      ],
    );
  }
  if (lower.contains('yem') || lower.contains('food')) {
    return _CategoryVisual(
      emoji: '🍜',
      gradient: [
        colorScheme.secondary,
        Color.lerp(colorScheme.secondary, Colors.black, 0.34)!,
      ],
    );
  }
  if (lower.contains('doğa') ||
      lower.contains('doga') ||
      lower.contains('nature')) {
    return _CategoryVisual(
      emoji: '🌿',
      gradient: [
        Color.lerp(colorScheme.primary, colorScheme.secondary, 0.28)!,
        Color.lerp(colorScheme.primary, Colors.black, 0.42)!,
      ],
    );
  }
  return _CategoryVisual(
    emoji: '🎲',
    gradient: [
      colorScheme.primary,
      Color.lerp(colorScheme.primary, colorScheme.secondary, 0.38)!,
    ],
  );
}
