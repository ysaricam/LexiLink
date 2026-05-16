import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/app/theme/app_layout.dart';
import 'package:lexilink_app/features/game/application/game_details_cubit.dart';
import 'package:lexilink_app/features/game/data/game_details.dart';
import 'package:lexilink_app/features/game/data/game_repository.dart';
import 'package:lexilink_app/features/game/data/outgoing_link.dart';
import 'package:lexilink_app/features/game/presentation/widgets/game_info_card.dart';
import 'package:lexilink_app/features/game/presentation/widgets/link_tile.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';
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
          return const AppScreen(
            child: AppErrorState(
              title: 'Session storage failed',
              message: 'Restart the app and try again.',
            ),
          );
        }

        final tokenStore = snapshot.data;
        if (tokenStore == null) {
          return const AppScreen(
            child: AppLoadingState(message: 'Preparing game...'),
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
    unawaited(_gameDetailsCubit.loadGame(widget.gameId));
  }

  @override
  void dispose() {
    _gameDetailsCubit.close();
    _httpClient.close();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return BlocProvider.value(
      value: _gameDetailsCubit,
      child: _GameView(gameId: widget.gameId),
    );
  }
}

class _GameView extends StatelessWidget {
  const _GameView({required this.gameId});

  final String gameId;

  @override
  Widget build(BuildContext context) {
    return AppScreen(
      size: AppScreenSize.game,
      child: BlocBuilder<GameDetailsCubit, GameDetailsState>(
        builder: (context, state) {
          if (state.status == GameDetailsStatus.loading) {
            return const AppLoadingState(message: 'Loading game...');
          }

          if (state.status == GameDetailsStatus.failure) {
            return AppErrorState(
              title: 'Could not load game',
              message: state.message ?? 'Try again.',
              onRetry: () => context.read<GameDetailsCubit>().loadGame(gameId),
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
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text(
          'LexiLink',
          style: Theme.of(context).textTheme.displaySmall,
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: 8),
        Text(
          '${game.difficulty} · ${game.state}',
          style: Theme.of(context).textTheme.bodyLarge,
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: 24),
        Wrap(
          alignment: WrapAlignment.center,
          spacing: 8,
          runSpacing: 8,
          children: [
            LinkTile(label: game.startWord, tone: LinkTileTone.current),
            LinkTile(label: game.currentWord),
            LinkTile(label: game.targetWord, tone: LinkTileTone.target),
          ],
        ),
        const SizedBox(height: 24),
        Row(
          children: [
            Expanded(
              child: GameInfoCard(
                label: 'Steps left',
                value: game.stepsLeft.toString(),
              ),
            ),
            const SizedBox(width: 8),
            Expanded(
              child: GameInfoCard(
                label: 'Hints',
                value: game.hintsLeft.toString(),
                accented: true,
              ),
            ),
          ],
        ),
        const SizedBox(height: 8),
        Row(
          children: [
            Expanded(
              child: GameInfoCard(
                label: 'Undo',
                value: game.undosLeft.toString(),
              ),
            ),
            const SizedBox(width: 8),
            Expanded(
              child: GameInfoCard(
                label: 'Reset',
                value: game.resetsLeft.toString(),
              ),
            ),
          ],
        ),
        if (game.score != null) ...[
          const SizedBox(height: 8),
          GameInfoCard(
            label: 'Score',
            value: game.score.toString(),
            accented: true,
          ),
        ],
        if (game.history.isNotEmpty) ...[
          const SizedBox(height: 16),
          _PathHistory(game: game),
        ],
        const SizedBox(height: 16),
        _GameActions(game: game, activeAction: activeAction),
        if (game.isFinished) ...[
          const SizedBox(height: 16),
          _GameResult(game: game),
        ],
        const SizedBox(height: 24),
        Text(
          'Next link',
          style: Theme.of(context).textTheme.titleMedium,
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: 12),
        if (outgoingLinks.isEmpty)
          const AppErrorState(
            title: 'No moves available',
            message: 'This link has no outgoing choices.',
          )
        else
          Wrap(
            alignment: WrapAlignment.center,
            spacing: 8,
            runSpacing: 8,
            children: [
              for (final link in outgoingLinks)
                _OutgoingLinkChoice(
                  link: link,
                  highlighted: link.id == recommendedLinkId,
                  disabled:
                      activeAction != GameAction.none ||
                      !link.isActive ||
                      game.isFinished,
                ),
            ],
          ),
        if (activeAction != GameAction.none) ...[
          const SizedBox(height: 16),
          AppLoadingState(message: _actionMessage(activeAction), compact: true),
        ],
        if (message != null) ...[
          const SizedBox(height: 16),
          AppErrorState(
            title: 'Action failed',
            message: message!,
          ),
        ],
      ],
    );
  }

  String _actionMessage(GameAction action) {
    return switch (action) {
      GameAction.step => 'Making step...',
      GameAction.hint => 'Finding hint...',
      GameAction.undo => 'Undoing...',
      GameAction.reset => 'Resetting...',
      GameAction.abandon => 'Abandoning...',
      GameAction.none => 'Working...',
    };
  }
}

class _PathHistory extends StatelessWidget {
  const _PathHistory({required this.game});

  final GameDetails game;

  @override
  Widget build(BuildContext context) {
    return Wrap(
      alignment: WrapAlignment.center,
      spacing: 8,
      runSpacing: 8,
      children: [
        LinkTile(label: game.startWord, tone: LinkTileTone.current),
        for (final step in game.history) LinkTile(label: step.linkValue),
      ],
    );
  }
}

class _GameActions extends StatelessWidget {
  const _GameActions({
    required this.game,
    required this.activeAction,
  });

  final GameDetails game;
  final GameAction activeAction;

  @override
  Widget build(BuildContext context) {
    final isBusy = activeAction != GameAction.none;
    final isFinished = game.isFinished;

    return Wrap(
      alignment: WrapAlignment.center,
      spacing: 8,
      runSpacing: 8,
      children: [
        AppSecondaryButton(
          label: 'Hint',
          onPressed: isBusy || isFinished || game.hintsLeft <= 0
              ? null
              : () => context.read<GameDetailsCubit>().useHint(),
        ),
        AppSecondaryButton(
          label: 'Undo',
          onPressed: isBusy || isFinished || game.undosLeft <= 0
              ? null
              : () => context.read<GameDetailsCubit>().undo(),
        ),
        AppSecondaryButton(
          label: 'Reset',
          onPressed: isBusy || isFinished || game.resetsLeft <= 0
              ? null
              : () => context.read<GameDetailsCubit>().reset(),
        ),
        AppDangerButton(
          label: 'Abandon',
          onPressed: isBusy || isFinished
              ? null
              : () => context.read<GameDetailsCubit>().abandon(),
        ),
      ],
    );
  }
}

class _GameResult extends StatelessWidget {
  const _GameResult({required this.game});

  final GameDetails game;

  @override
  Widget build(BuildContext context) {
    final title = switch (game.state) {
      'Completed' => 'Completed',
      'Failed' => 'Failed',
      'Abandoned' => 'Abandoned',
      _ => game.state,
    };
    final message = switch (game.state) {
      'Completed' => 'You reached ${game.targetWord}.',
      'Failed' => 'No steps left.',
      'Abandoned' => 'This game was abandoned.',
      _ => 'Game ended.',
    };

    return DecoratedBox(
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.secondaryContainer,
        borderRadius: BorderRadius.circular(8),
      ),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            Text(title, style: Theme.of(context).textTheme.titleMedium),
            const SizedBox(height: 6),
            Text(
              message,
              style: Theme.of(context).textTheme.bodyMedium,
              textAlign: TextAlign.center,
            ),
          ],
        ),
      ),
    );
  }
}

class _OutgoingLinkChoice extends StatelessWidget {
  const _OutgoingLinkChoice({
    required this.link,
    required this.highlighted,
    required this.disabled,
  });

  final OutgoingLink link;
  final bool highlighted;
  final bool disabled;

  @override
  Widget build(BuildContext context) {
    return LinkTile(
      label: link.value,
      tone: disabled
          ? LinkTileTone.disabled
          : highlighted
          ? LinkTileTone.target
          : LinkTileTone.normal,
      onPressed: disabled
          ? null
          : () => context.read<GameDetailsCubit>().makeStep(link.id),
    );
  }
}
