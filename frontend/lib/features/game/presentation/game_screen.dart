import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/app/theme/app_layout.dart';
import 'package:lexilink_app/app/theme/app_palette.dart';
import 'package:lexilink_app/features/game/application/game_details_cubit.dart';
import 'package:lexilink_app/features/game/application/game_sound_effects.dart';
import 'package:lexilink_app/features/game/data/game_details.dart';
import 'package:lexilink_app/features/game/data/game_repository.dart';
import 'package:lexilink_app/features/game/data/outgoing_link.dart';
import 'package:lexilink_app/features/hint/application/hint_cubit.dart';
import 'package:lexilink_app/features/hint/data/hint_repository.dart';
import 'package:lexilink_app/features/reset/application/reset_cubit.dart';
import 'package:lexilink_app/features/reset/data/reset_repository.dart';
import 'package:lexilink_app/features/undo/application/undo_cubit.dart';
import 'package:lexilink_app/features/undo/data/undo_repository.dart';
import 'package:lexilink_app/shared/ads/ad_config.dart';
import 'package:lexilink_app/shared/ads/ads_service.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/audio/audio_service.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';
import 'package:lexilink_app/shared/widgets/app_back_bar.dart';
import 'package:lexilink_app/shared/widgets/app_button.dart';
import 'package:lexilink_app/shared/widgets/app_error_state.dart';
import 'package:lexilink_app/shared/widgets/app_loading_state.dart';
import 'package:lexilink_app/shared/widgets/app_screen.dart';

class GameScreen extends StatefulWidget {
  const GameScreen({
    required this.gameId,
    super.key,
  });

  final String gameId;

  @override
  State<GameScreen> createState() => _GameScreenState();
}

class _GameScreenState extends State<GameScreen> {
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
          return AppScreen(
            child: AppErrorState(
              title: context.l10n.sessionStorageFailedTitle,
              message: context.l10n.sessionStorageFailedMessage,
            ),
          );
        }

        final tokenStore = snapshot.data;
        if (tokenStore == null) {
          return AppScreen(
            child: AppLoadingState(message: context.l10n.preparingGame),
          );
        }

        return _GameProviders(gameId: widget.gameId, tokenStore: tokenStore);
      },
    );
  }
}

class _GameProviders extends StatefulWidget {
  const _GameProviders({
    required this.gameId,
    required this.tokenStore,
  });

  final String gameId;
  final TokenStore tokenStore;

  @override
  State<_GameProviders> createState() => _GameProvidersState();
}

class _GameProvidersState extends State<_GameProviders> {
  late final http.Client _httpClient;
  late final GameDetailsCubit _gameDetailsCubit;
  late final HintCubit _hintCubit;
  late final UndoCubit _undoCubit;
  late final ResetCubit _resetCubit;

  @override
  void initState() {
    super.initState();
    _httpClient = http.Client();
    final apiClient = ApiClient(
      config: ApiConfig.local(),
      httpClient: _httpClient,
      tokenStore: widget.tokenStore,
    );
    _gameDetailsCubit = GameDetailsCubit(
      gameRepository: GameRepository(apiClient: apiClient),
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
    unawaited(_gameDetailsCubit.loadGame(widget.gameId));
    unawaited(_hintCubit.loadHint());
    unawaited(_undoCubit.loadUndo());
    unawaited(_resetCubit.loadReset());
  }

  @override
  void dispose() {
    _resetCubit.close();
    _undoCubit.close();
    _hintCubit.close();
    _gameDetailsCubit.close();
    _httpClient.close();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return MultiBlocProvider(
      providers: [
        BlocProvider.value(value: _gameDetailsCubit),
        BlocProvider.value(value: _hintCubit),
        BlocProvider.value(value: _undoCubit),
        BlocProvider.value(value: _resetCubit),
      ],
      child: _GameView(gameId: widget.gameId),
    );
  }
}

class _GameView extends StatefulWidget {
  const _GameView({required this.gameId});

  final String gameId;

  @override
  State<_GameView> createState() => _GameViewState();
}

class _GameViewState extends State<_GameView> {
  bool _resultShown = false;
  GameDetailsState? _previousState;

  @override
  Widget build(BuildContext context) {
    return MultiBlocListener(
      listeners: [
        BlocListener<GameDetailsCubit, GameDetailsState>(
          listener: (context, state) {
            _playActionAudio(context, _previousState, state);
            _previousState = state;
          },
        ),
        BlocListener<GameDetailsCubit, GameDetailsState>(
          listenWhen: (previous, current) {
            final wasFinished = previous.game?.isFinished ?? false;
            final isFinished = current.game?.isFinished ?? false;
            return !wasFinished && isFinished;
          },
          listener: (context, state) {
            final game = state.game;
            if (game == null || _resultShown) return;
            _resultShown = true;
            // Interstitial @ game end (~1/2). Fire-and-forget and web-safe;
            // never blocks the result sheet.
            context.read<AdsService>().maybeShowInterstitial(
              InterstitialChance.gameEnd,
            );
            _showResultSheet(context, game);
          },
        ),
      ],
      child: AppScreen(
        size: AppScreenSize.game,
        child: BlocBuilder<GameDetailsCubit, GameDetailsState>(
          builder: (context, state) {
            if (state.status == GameDetailsStatus.loading) {
              return AppLoadingState(message: context.l10n.loadingGame);
            }

            if (state.status == GameDetailsStatus.failure) {
              return AppErrorState(
                title: context.l10n.couldNotLoadGame,
                message: state.message ?? context.l10n.commonTryAgain,
                onRetry: () =>
                    context.read<GameDetailsCubit>().loadGame(widget.gameId),
              );
            }

            return _GameContent(
              game: state.game!,
              outgoingLinks: state.outgoingLinks,
              activeAction: state.activeAction,
              recommendedLinkId: state.recommendedLinkId,
              message: state.message,
            );
          },
        ),
      ),
    );
  }

  void _playActionAudio(
    BuildContext context,
    GameDetailsState? previous,
    GameDetailsState current,
  ) {
    final effect = soundEffectForGameTransition(previous, current);
    if (effect != null) {
      context.read<AudioService>().playEffect(effect);
    }
  }

  Future<void> _showResultSheet(BuildContext context, GameDetails game) async {
    await showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (sheetContext) => _ResultSheet(game: game),
    );
  }
}

class _GameContent extends StatelessWidget {
  const _GameContent({
    required this.game,
    required this.outgoingLinks,
    required this.activeAction,
    required this.recommendedLinkId,
    required this.message,
  });

  final GameDetails game;
  final List<OutgoingLink> outgoingLinks;
  final GameAction activeAction;
  final String? recommendedLinkId;
  final String? message;

  @override
  Widget build(BuildContext context) {
    final isBusy = activeAction != GameAction.none;
    final isFinished = game.isFinished;
    final undoBalance = context.watch<UndoCubit>().state.undo?.balance ?? 0;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        AppBackBar(
          title: context.l10n.gameTitle,
          onBack: () => _onBackPressed(context, game, isBusy),
          trailing: _OverflowMenu(game: game, isBusy: isBusy),
        ),
        const SizedBox(height: 20),
        _StartRailTarget(game: game),
        const SizedBox(height: 24),
        _CurrentHero(game: game),
        const SizedBox(height: 10),
        _Breadcrumb(game: game),
        const SizedBox(height: 24),
        _StatusRow(game: game),
        const SizedBox(height: 20),
        if (!isFinished) ...[
          Text(
            context.l10n.pickNextWord,
            style: Theme.of(context).textTheme.titleMedium,
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 12),
          if (outgoingLinks.isEmpty)
            AppErrorState(
              title: context.l10n.noMovesTitle,
              message: context.l10n.noMovesMessage,
            )
          else
            _OptionsGrid(
              options: outgoingLinks,
              recommendedLinkId: recommendedLinkId,
              previousLinkId: game.backtrackParentLinkId,
              undoBalance: undoBalance,
              disabled: isBusy || isFinished,
            ),
          const SizedBox(height: 16),
          _SecondaryActions(game: game, isBusy: isBusy),
        ],
        if (message != null) ...[
          const SizedBox(height: 16),
          AppErrorState(
            title: context.l10n.actionFailed,
            message: message!,
          ),
        ],
        if (isBusy) ...[
          const SizedBox(height: 16),
          AppLoadingState(
            message: _actionMessage(context, activeAction),
            compact: true,
          ),
        ],
        if (isFinished) ...[
          const SizedBox(height: 24),
          AppPrimaryButton(
            label: context.l10n.backToHome,
            onPressed: () => context.go('/home'),
          ),
        ],
        const SizedBox(height: 24),
      ],
    );
  }

  String _actionMessage(BuildContext context, GameAction action) {
    final l10n = context.l10n;
    return switch (action) {
      GameAction.step => l10n.actionMakingStep,
      GameAction.hint => l10n.actionFindingHint,
      GameAction.undo => l10n.actionUndoing,
      GameAction.reset => l10n.actionResetting,
      GameAction.abandon => l10n.actionAbandoning,
      GameAction.none => l10n.actionWorking,
    };
  }

  Future<void> _onBackPressed(
    BuildContext context,
    GameDetails game,
    bool isBusy,
  ) async {
    if (game.isFinished) {
      context.go('/home');
      return;
    }
    if (isBusy) return;

    final confirmed = await showDialog<bool>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: Text(context.l10n.quitGameTitle),
        content: Text(context.l10n.quitGameMessage),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(false),
            child: Text(context.l10n.keepPlaying),
          ),
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(true),
            child: Text(context.l10n.quit),
          ),
        ],
      ),
    );

    if ((confirmed ?? false) && context.mounted) {
      await context.read<GameDetailsCubit>().abandon();
    }
  }
}

class _StartRailTarget extends StatelessWidget {
  const _StartRailTarget({required this.game});

  final GameDetails game;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        _AnchorChip(label: game.startWord, kind: _AnchorKind.start),
        const SizedBox(width: 10),
        Expanded(
          child: _StepDots(
            stepsTaken: game.stepsTaken,
            maxSteps: game.maxSteps,
          ),
        ),
        const SizedBox(width: 10),
        _AnchorChip(label: game.targetWord, kind: _AnchorKind.target),
      ],
    );
  }
}

enum _AnchorKind { start, target }

class _AnchorChip extends StatelessWidget {
  const _AnchorChip({required this.label, required this.kind});

  final String label;
  final _AnchorKind kind;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final background = kind == _AnchorKind.start
        ? AppPalette.primarySoft
        : AppPalette.focusSoft;
    final foreground = kind == _AnchorKind.start
        ? AppPalette.primary
        : AppPalette.focus;
    final caption = kind == _AnchorKind.start
        ? context.l10n.commonStart
        : context.l10n.anchorTarget;

    return ConstrainedBox(
      constraints: const BoxConstraints(maxWidth: 120),
      child: Column(
        children: [
          Text(
            caption,
            style: textTheme.labelSmall?.copyWith(color: foreground),
          ),
          const SizedBox(height: 4),
          DecoratedBox(
            decoration: BoxDecoration(
              color: background,
              borderRadius: BorderRadius.circular(8),
              border: Border.all(color: foreground.withValues(alpha: 0.4)),
            ),
            child: Padding(
              padding: const EdgeInsets.symmetric(
                horizontal: 10,
                vertical: 6,
              ),
              child: Text(
                label,
                style: textTheme.titleSmall?.copyWith(color: foreground),
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _StepDots extends StatelessWidget {
  const _StepDots({required this.stepsTaken, required this.maxSteps});

  final int stepsTaken;
  final int maxSteps;

  @override
  Widget build(BuildContext context) {
    if (maxSteps <= 0) return const SizedBox.shrink();

    return LayoutBuilder(
      builder: (context, constraints) {
        const dotSize = 10.0;
        final spacing = maxSteps > 1
            ? (constraints.maxWidth - dotSize * maxSteps) / (maxSteps - 1)
            : 0.0;
        final safeSpacing = spacing.isFinite && spacing > 0 ? spacing : 4.0;

        return Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: List<Widget>.generate(maxSteps, (index) {
            final filled = index < stepsTaken;
            return Padding(
              padding: EdgeInsets.only(
                right: index == maxSteps - 1 ? 0 : safeSpacing / 2,
                left: index == 0 ? 0 : safeSpacing / 2,
              ),
              child: Container(
                width: dotSize,
                height: dotSize,
                decoration: BoxDecoration(
                  color: filled
                      ? AppPalette.focus
                      : AppPalette.primary.withValues(alpha: 0.18),
                  shape: BoxShape.circle,
                ),
              ),
            );
          }),
        );
      },
    );
  }
}

class _CurrentHero extends StatelessWidget {
  const _CurrentHero({required this.game});

  final GameDetails game;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Center(
      child: ConstrainedBox(
        constraints: const BoxConstraints(maxWidth: 360),
        child: Container(
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 22),
          decoration: BoxDecoration(
            gradient: const LinearGradient(
              begin: Alignment.topLeft,
              end: Alignment.bottomRight,
              colors: [AppPalette.primary, AppPalette.primaryPressed],
            ),
            borderRadius: BorderRadius.circular(20),
            boxShadow: [
              BoxShadow(
                color: AppPalette.primary.withValues(alpha: 0.22),
                blurRadius: 14,
                offset: const Offset(0, 6),
              ),
            ],
          ),
          child: Column(
            children: [
              Text(
                context.l10n.currentLabel,
                style: theme.textTheme.labelMedium?.copyWith(
                  color: Colors.white70,
                  letterSpacing: 1.2,
                ),
              ),
              const SizedBox(height: 6),
              FittedBox(
                fit: BoxFit.scaleDown,
                child: Text(
                  game.currentWord,
                  style: theme.textTheme.headlineMedium?.copyWith(
                    color: Colors.white,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _Breadcrumb extends StatelessWidget {
  const _Breadcrumb({required this.game});

  final GameDetails game;

  @override
  Widget build(BuildContext context) {
    final words = <String>[
      game.startWord,
      ...game.history.map((h) => h.linkValue),
    ];
    final tail = words.length <= 3 ? words : words.sublist(words.length - 3);

    final theme = Theme.of(context);
    return Center(
      child: Text(
        tail.join(' › '),
        style: theme.textTheme.bodySmall?.copyWith(
          color: AppPalette.lightTextMuted,
        ),
        maxLines: 1,
        overflow: TextOverflow.ellipsis,
      ),
    );
  }
}

class _StatusRow extends StatelessWidget {
  const _StatusRow({required this.game});

  final GameDetails game;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        _StatusChip(
          icon: Icons.timer_outlined,
          label: context.l10n.hudSteps(game.stepsTaken, game.maxSteps),
        ),
        const SizedBox(width: 10),
        _StatusChip(
          icon: Icons.lightbulb_outline,
          label: context.l10n.hudHints(game.hintsLeft),
        ),
        if (game.score != null) ...[
          const SizedBox(width: 10),
          _StatusChip(
            icon: Icons.star_outline,
            label: context.l10n.hudScore(game.score!),
            tone: _StatusChipTone.accent,
          ),
        ],
      ],
    );
  }
}

enum _StatusChipTone { neutral, accent }

class _StatusChip extends StatelessWidget {
  const _StatusChip({
    required this.icon,
    required this.label,
    this.tone = _StatusChipTone.neutral,
  });

  final IconData icon;
  final String label;
  final _StatusChipTone tone;

  @override
  Widget build(BuildContext context) {
    final isAccent = tone == _StatusChipTone.accent;
    final background = isAccent ? AppPalette.focusSoft : AppPalette.primarySoft;
    final foreground = isAccent ? AppPalette.focus : AppPalette.primary;

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
      decoration: BoxDecoration(
        color: background,
        borderRadius: BorderRadius.circular(999),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 14, color: foreground),
          const SizedBox(width: 6),
          Text(
            label,
            style: Theme.of(context).textTheme.labelMedium?.copyWith(
              color: foreground,
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      ),
    );
  }
}

class _OptionsGrid extends StatelessWidget {
  const _OptionsGrid({
    required this.options,
    required this.recommendedLinkId,
    required this.previousLinkId,
    required this.undoBalance,
    required this.disabled,
  });

  final List<OutgoingLink> options;
  final String? recommendedLinkId;
  final String? previousLinkId;
  final int undoBalance;
  final bool disabled;

  @override
  Widget build(BuildContext context) {
    return GridView.builder(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: 3,
        mainAxisSpacing: 10,
        crossAxisSpacing: 10,
        childAspectRatio: 1.6,
      ),
      itemCount: options.length,
      itemBuilder: (context, index) {
        final option = options[index];
        final isRecommended = option.id == recommendedLinkId;
        final isPrevious =
            previousLinkId != null && option.id == previousLinkId;
        final tileDisabled =
            disabled || !option.isActive || (isPrevious && undoBalance <= 0);
        return _OptionTile(
          option: option,
          highlighted: isRecommended,
          isPrevious: isPrevious,
          disabled: tileDisabled,
        );
      },
    );
  }
}

class _OptionTile extends StatelessWidget {
  const _OptionTile({
    required this.option,
    required this.highlighted,
    required this.isPrevious,
    required this.disabled,
  });

  final OutgoingLink option;
  final bool highlighted;
  final bool isPrevious;
  final bool disabled;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final Color background;
    final Color foreground;
    final Color border;
    final double borderWidth;

    if (disabled) {
      background = AppPalette.lightSurfaceMuted;
      foreground = AppPalette.lightTextMuted;
      border = AppPalette.primary.withValues(alpha: 0.18);
      borderWidth = 1;
    } else if (highlighted) {
      background = AppPalette.focusSoft;
      foreground = AppPalette.focus;
      border = AppPalette.focus;
      borderWidth = 2;
    } else if (isPrevious) {
      background = AppPalette.primarySoft;
      foreground = AppPalette.primary;
      border = AppPalette.primary.withValues(alpha: 0.44);
      borderWidth = 2;
    } else {
      background = AppPalette.lightSurface;
      foreground = AppPalette.lightText;
      border = AppPalette.primary.withValues(alpha: 0.28);
      borderWidth = 1;
    }

    return Material(
      color: background,
      borderRadius: BorderRadius.circular(12),
      child: InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: disabled
            ? null
            : () async {
                if (isPrevious) {
                  await context.read<GameDetailsCubit>().undo();
                  if (context.mounted) {
                    await context.read<UndoCubit>().loadUndo();
                  }
                  return;
                }

                await context.read<GameDetailsCubit>().makeStep(option.id);
              },
        child: DecoratedBox(
          decoration: BoxDecoration(
            border: Border.all(color: border, width: borderWidth),
            borderRadius: BorderRadius.circular(12),
          ),
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
            child: Stack(
              children: [
                if (isPrevious)
                  Positioned(
                    top: 0,
                    right: 0,
                    child: Icon(
                      Icons.undo,
                      size: 14,
                      color: foreground.withValues(alpha: 0.6),
                    ),
                  ),
                Center(
                  child: FittedBox(
                    fit: BoxFit.scaleDown,
                    child: Text(
                      option.value,
                      style: theme.textTheme.titleMedium?.copyWith(
                        color: foreground,
                        fontWeight: highlighted
                            ? FontWeight.w700
                            : FontWeight.w500,
                      ),
                      textAlign: TextAlign.center,
                      maxLines: 2,
                    ),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _SecondaryActions extends StatelessWidget {
  const _SecondaryActions({required this.game, required this.isBusy});

  final GameDetails game;
  final bool isBusy;

  @override
  Widget build(BuildContext context) {
    final hintBalance = context.watch<HintCubit>().state.hint?.balance ?? 0;
    return Row(
      children: [
        Expanded(
          child: AppSecondaryButton(
            label: context.l10n.hintAction(hintBalance),
            onPressed: isBusy || game.isFinished || hintBalance <= 0
                ? null
                : () async {
                    await context.read<GameDetailsCubit>().useHint();
                    if (context.mounted) {
                      await context.read<HintCubit>().loadHint();
                    }
                  },
          ),
        ),
      ],
    );
  }
}

class _OverflowMenu extends StatelessWidget {
  const _OverflowMenu({required this.game, required this.isBusy});

  final GameDetails game;
  final bool isBusy;

  @override
  Widget build(BuildContext context) {
    final resetBalance = context.watch<ResetCubit>().state.reset?.balance ?? 0;
    final resetEnabled = !game.isFinished && !isBusy && resetBalance > 0;

    return PopupMenuButton<String>(
      icon: const Icon(Icons.more_vert),
      tooltip: context.l10n.moreActions,
      onSelected: (value) async {
        if (value == 'reset') {
          await context.read<GameDetailsCubit>().reset();
          if (context.mounted) {
            await context.read<ResetCubit>().loadReset();
          }
        }
      },
      itemBuilder: (popupContext) => [
        PopupMenuItem<String>(
          value: 'reset',
          enabled: resetEnabled,
          child: Text(context.l10n.resetProgress(resetBalance)),
        ),
      ],
    );
  }
}

class _ResultSheet extends StatelessWidget {
  const _ResultSheet({required this.game});

  final GameDetails game;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final outcome = _outcomeFor(context, game.state);

    return SafeArea(
      child: Container(
        decoration: BoxDecoration(
          color: AppPalette.lightSurface,
          borderRadius: const BorderRadius.vertical(top: Radius.circular(24)),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(alpha: 0.08),
              blurRadius: 18,
              offset: const Offset(0, -4),
            ),
          ],
        ),
        padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Center(
              child: Container(
                width: 40,
                height: 4,
                decoration: BoxDecoration(
                  color: AppPalette.lightSurfaceMuted,
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
            ),
            const SizedBox(height: 16),
            Row(
              children: [
                Icon(outcome.icon, color: outcome.color, size: 28),
                const SizedBox(width: 10),
                Text(
                  outcome.title,
                  style: theme.textTheme.headlineSmall?.copyWith(
                    color: outcome.color,
                    fontWeight: FontWeight.w700,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 6),
            Text(
              outcome.subtitle(game),
              style: theme.textTheme.bodyMedium?.copyWith(
                color: AppPalette.lightTextMuted,
              ),
            ),
            const SizedBox(height: 18),
            Row(
              children: [
                Expanded(
                  child: _SummaryStat(
                    label: context.l10n.resultScore,
                    value: game.score?.toString() ?? '—',
                  ),
                ),
                Expanded(
                  child: _SummaryStat(
                    label: context.l10n.resultSteps,
                    value: '${game.stepsTaken}/${game.maxSteps}',
                  ),
                ),
                Expanded(
                  child: _SummaryStat(
                    label: context.l10n.resultHintsUsed,
                    value: '${game.hintsUsed}',
                  ),
                ),
              ],
            ),
            const SizedBox(height: 18),
            Text(
              context.l10n.resultPath,
              style: theme.textTheme.titleSmall,
            ),
            const SizedBox(height: 6),
            Text(
              _pathFor(game),
              style: theme.textTheme.bodyMedium,
            ),
            const SizedBox(height: 22),
            AppPrimaryButton(
              label: context.l10n.backToHome,
              onPressed: () {
                Navigator.of(context).pop();
                context.go('/home');
              },
            ),
          ],
        ),
      ),
    );
  }

  String _pathFor(GameDetails game) {
    final words = <String>[
      game.startWord,
      ...game.history.map((h) => h.linkValue),
    ];
    return words.join(' › ');
  }

  _Outcome _outcomeFor(BuildContext context, String state) {
    final l10n = context.l10n;
    return switch (state) {
      'Completed' => _Outcome(
        icon: Icons.emoji_events_outlined,
        color: AppPalette.success,
        title: l10n.outcomeCompletedTitle,
        subtitle: (g) => l10n.outcomeCompletedSubtitle(g.targetWord),
      ),
      'Failed' => _Outcome(
        icon: Icons.do_not_disturb_alt_outlined,
        color: AppPalette.danger,
        title: l10n.outcomeFailedTitle,
        subtitle: (g) => l10n.outcomeFailedSubtitle(g.targetWord),
      ),
      'Abandoned' => _Outcome(
        icon: Icons.flag_outlined,
        color: AppPalette.lightTextMuted,
        title: l10n.outcomeAbandonedTitle,
        subtitle: (_) => l10n.outcomeAbandonedSubtitle,
      ),
      _ => _Outcome(
        icon: Icons.help_outline,
        color: AppPalette.lightTextMuted,
        title: state,
        subtitle: (_) => l10n.outcomeEndedSubtitle,
      ),
    };
  }
}

class _Outcome {
  const _Outcome({
    required this.icon,
    required this.color,
    required this.title,
    required this.subtitle,
  });

  final IconData icon;
  final Color color;
  final String title;
  final String Function(GameDetails) subtitle;
}

class _SummaryStat extends StatelessWidget {
  const _SummaryStat({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          label,
          style: theme.textTheme.labelSmall?.copyWith(
            color: AppPalette.lightTextMuted,
          ),
        ),
        const SizedBox(height: 2),
        Text(
          value,
          style: theme.textTheme.titleMedium?.copyWith(
            fontWeight: FontWeight.w700,
          ),
        ),
      ],
    );
  }
}
