import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:http/http.dart' as http;
import 'package:lexilink_app/features/quests/application/quests_cubit.dart';
import 'package:lexilink_app/features/quests/data/player_quest.dart';
import 'package:lexilink_app/features/quests/data/quest_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/audio/audio_service.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';
import 'package:lexilink_app/shared/widgets/app_back_bar.dart';
import 'package:lexilink_app/shared/widgets/app_button.dart';
import 'package:lexilink_app/shared/widgets/app_empty_state.dart';
import 'package:lexilink_app/shared/widgets/app_error_state.dart';
import 'package:lexilink_app/shared/widgets/app_loading_state.dart';
import 'package:lexilink_app/shared/widgets/app_screen.dart';

class QuestsScreen extends StatefulWidget {
  const QuestsScreen({super.key});

  @override
  State<QuestsScreen> createState() => _QuestsScreenState();
}

class _QuestsScreenState extends State<QuestsScreen> {
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
            child: AppLoadingState(message: context.l10n.preparingSession),
          );
        }

        return _QuestsProviders(tokenStore: tokenStore);
      },
    );
  }
}

class _QuestsProviders extends StatefulWidget {
  const _QuestsProviders({required this.tokenStore});

  final TokenStore tokenStore;

  @override
  State<_QuestsProviders> createState() => _QuestsProvidersState();
}

class _QuestsProvidersState extends State<_QuestsProviders> {
  late final http.Client _httpClient;
  late final QuestsCubit _questsCubit;

  @override
  void initState() {
    super.initState();
    _httpClient = http.Client();
    final apiClient = ApiClient(
      config: ApiConfig.local(),
      httpClient: _httpClient,
      tokenStore: widget.tokenStore,
    );
    _questsCubit = QuestsCubit(
      questRepository: QuestRepository(apiClient: apiClient),
    );
    unawaited(_questsCubit.loadQuests());
  }

  @override
  void dispose() {
    _questsCubit.close();
    _httpClient.close();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return BlocProvider.value(
      value: _questsCubit,
      child: const _QuestsView(),
    );
  }
}

class _QuestsView extends StatelessWidget {
  const _QuestsView();

  @override
  Widget build(BuildContext context) {
    return AppScreen(
      child: BlocConsumer<QuestsCubit, QuestsState>(
        listenWhen: (previous, current) =>
            previous.claimMessage != current.claimMessage &&
            current.claimMessage != null,
        listener: (context, state) {
          final message = state.claimMessage;
          if (message == null) {
            return;
          }
          ScaffoldMessenger.of(context)
            ..clearSnackBars()
            ..showSnackBar(SnackBar(content: Text(message)));
          context.read<QuestsCubit>().clearClaimMessage();
        },
        builder: (context, state) {
          return Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              AppBackBar(title: context.l10n.navQuests),
              const SizedBox(height: 8),
              Text(
                context.l10n.questsSubtitle,
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                  color: Theme.of(context).colorScheme.onSurfaceVariant,
                ),
              ),
              const SizedBox(height: 20),
              if (state.isLoading && state.quests.isEmpty)
                AppLoadingState(message: context.l10n.questsLoading)
              else if (state.status == QuestsStatus.failure)
                AppErrorState(
                  title: context.l10n.questsLoadError,
                  message: state.message ?? context.l10n.commonTryAgain,
                  onRetry: () => context.read<QuestsCubit>().loadQuests(),
                )
              else if (state.status == QuestsStatus.success &&
                  state.quests.isEmpty)
                AppEmptyState(
                  title: context.l10n.noQuestsTitle,
                  message: context.l10n.noQuestsMessage,
                  actionLabel: context.l10n.commonRefresh,
                  onAction: () => context.read<QuestsCubit>().loadQuests(),
                )
              else
                _QuestsList(quests: state.quests, claimingId: state.claimingId),
            ],
          );
        },
      ),
    );
  }
}

class _QuestsList extends StatelessWidget {
  const _QuestsList({
    required this.quests,
    required this.claimingId,
  });

  final List<PlayerQuest> quests;
  final String? claimingId;

  @override
  Widget build(BuildContext context) {
    final sorted = [...quests]..sort(_questOrdering);

    return Column(
      children: [
        for (var index = 0; index < sorted.length; index++) ...[
          _QuestTile(
            quest: sorted[index],
            isClaiming: claimingId == sorted[index].id,
            claimDisabled: claimingId != null && claimingId != sorted[index].id,
          ),
          if (index < sorted.length - 1) const SizedBox(height: 10),
        ],
      ],
    );
  }

  static int _questOrdering(PlayerQuest a, PlayerQuest b) {
    int rank(PlayerQuest q) {
      switch (q.state) {
        case QuestState.readyToClaim:
          return 0;
        case QuestState.active:
          return 1;
        case QuestState.claimed:
          return 2;
        case QuestState.unknown:
          return 3;
      }
    }

    final byRank = rank(a) - rank(b);
    if (byRank != 0) {
      return byRank;
    }
    return b.issuedAt.compareTo(a.issuedAt);
  }
}

class _QuestTile extends StatelessWidget {
  const _QuestTile({
    required this.quest,
    required this.isClaiming,
    required this.claimDisabled,
  });

  final PlayerQuest quest;
  final bool isClaiming;
  final bool claimDisabled;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final colorScheme = Theme.of(context).colorScheme;
    final progressFraction = quest.threshold == 0
        ? 0.0
        : (quest.progress / quest.threshold).clamp(0.0, 1.0);

    return DecoratedBox(
      decoration: BoxDecoration(
        color: colorScheme.surface,
        border: Border.all(color: colorScheme.outline.withValues(alpha: 0.28)),
        borderRadius: BorderRadius.circular(16),
      ),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(
                    quest.name,
                    style: textTheme.titleMedium,
                  ),
                ),
                _StateBadge(state: quest.state),
              ],
            ),
            if (quest.description.isNotEmpty) ...[
              const SizedBox(height: 4),
              Text(
                quest.description,
                style: textTheme.bodySmall?.copyWith(
                  color: colorScheme.onSurfaceVariant,
                ),
              ),
            ],
            const SizedBox(height: 10),
            ClipRRect(
              borderRadius: BorderRadius.circular(999),
              child: LinearProgressIndicator(
                value: progressFraction,
                minHeight: 6,
                backgroundColor: colorScheme.outline.withValues(alpha: 0.18),
                color: colorScheme.primary,
              ),
            ),
            const SizedBox(height: 8),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  '${quest.progress}/${quest.threshold}',
                  style: textTheme.bodySmall?.copyWith(
                    color: colorScheme.onSurfaceVariant,
                  ),
                ),
                Wrap(
                  spacing: 8,
                  children: [
                    if (quest.energyReward > 0)
                      Text(
                        '+${quest.energyReward}⚡',
                        style: textTheme.labelLarge?.copyWith(
                          color: colorScheme.primary,
                        ),
                      ),
                    if (quest.hintReward > 0)
                      Text(
                        '+${quest.hintReward}💡',
                        style: textTheme.labelLarge?.copyWith(
                          color: colorScheme.tertiary,
                        ),
                      ),
                    if (quest.undoReward > 0)
                      Text(
                        '+${quest.undoReward}↶',
                        style: textTheme.labelLarge?.copyWith(
                          color: colorScheme.secondary,
                        ),
                      ),
                    if (quest.resetReward > 0)
                      Text(
                        '+${quest.resetReward}↻',
                        style: textTheme.labelLarge?.copyWith(
                          color: colorScheme.error,
                        ),
                      ),
                    if (quest.diamondReward > 0)
                      Text(
                        '+${quest.diamondReward}💎',
                        style: textTheme.labelLarge?.copyWith(
                          color: colorScheme.primary,
                        ),
                      ),
                  ],
                ),
              ],
            ),
            if (quest.isReadyToClaim) ...[
              const SizedBox(height: 12),
              AppPrimaryButton(
                label: isClaiming
                    ? context.l10n.questClaiming
                    : context.l10n.questClaimReward,
                onPressed: (isClaiming || claimDisabled)
                    ? null
                    : () async {
                        final audio = context.read<AudioService>();
                        final cubit = context.read<QuestsCubit>();
                        final claimed = await cubit.claim(quest.id);
                        await audio.playEffect(
                          claimed
                              ? SoundEffect.questClaim
                              : SoundEffect.error,
                        );
                      },
              ),
            ],
          ],
        ),
      ),
    );
  }
}

class _StateBadge extends StatelessWidget {
  const _StateBadge({required this.state});

  final QuestState state;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;
    final textTheme = Theme.of(context).textTheme;

    final (label, foreground, background) = switch (state) {
      QuestState.readyToClaim => (
        context.l10n.questStateReady,
        colorScheme.onPrimary,
        colorScheme.primary,
      ),
      QuestState.active => (
        context.l10n.questStateActive,
        colorScheme.onSurface,
        colorScheme.surfaceContainerHighest,
      ),
      QuestState.claimed => (
        context.l10n.questStateClaimed,
        colorScheme.onSecondaryContainer,
        colorScheme.secondaryContainer,
      ),
      QuestState.unknown => (
        '—',
        colorScheme.onSurfaceVariant,
        colorScheme.surfaceContainerLow,
      ),
    };

    return DecoratedBox(
      decoration: BoxDecoration(
        color: background,
        borderRadius: BorderRadius.circular(999),
      ),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
        child: Text(
          label,
          style: textTheme.labelSmall?.copyWith(color: foreground),
        ),
      ),
    );
  }
}
